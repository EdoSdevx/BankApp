using System.Data;
using BankApp.BankApp.Common.Dtos.Eft.Switch;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class EftOutboxDataAccess : IEftOutboxDataAccess
{
    private readonly DatabaseContext _context;

    public EftOutboxDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<PendingEftOutboxDto>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_EftOutbox_Pending");
        command.Parameters.Add("@BatchSize", SqlDbType.Int).Value = batchSize;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var messages = new List<PendingEftOutboxDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            messages.Add(new PendingEftOutboxDto
            {
                OutboxMessageId = reader.GetInt32(reader.GetOrdinal("OutboxMessageId")),
                EftTransferId = reader.GetInt32(reader.GetOrdinal("EftTransferId")),
                AttemptCount = reader.GetInt32(reader.GetOrdinal("AttemptCount")),
                SenderReference = reader.GetString(reader.GetOrdinal("SenderReference")),
                ReceiverIban = reader.GetString(reader.GetOrdinal("ReceiverIban")),
                ReceiverName = reader.GetString(reader.GetOrdinal("ReceiverName")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Description = GetNullableString(reader, "Description")
            });
        }

        return messages;
    }

    public async Task MarkSubmittedAsync(
        int outboxMessageId,
        string centralReference,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_EftOutbox_MarkSubmitted");
        command.Parameters.Add("@OutboxMessageId", SqlDbType.Int).Value = outboxMessageId;
        command.Parameters.Add("@CentralReference", SqlDbType.VarChar, 64).Value = centralReference;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        int outboxMessageId,
        string error,
        int maxAttempts,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_EftOutbox_MarkFailed");
        command.Parameters.Add("@OutboxMessageId", SqlDbType.Int).Value = outboxMessageId;
        command.Parameters.Add("@Error", SqlDbType.NVarChar, 1000).Value =
            error[..Math.Min(error.Length, 1000)];
        command.Parameters.Add("@MaxAttempts", SqlDbType.Int).Value = maxAttempts;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SqlCommand CreateCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
