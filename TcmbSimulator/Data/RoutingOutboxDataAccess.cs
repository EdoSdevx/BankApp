using System.Data;
using Microsoft.Data.SqlClient;
using TcmbSimulator.Contracts.Routing;

namespace TcmbSimulator.Data;

public class RoutingOutboxDataAccess : IRoutingOutboxDataAccess
{
    private readonly TcmbDatabaseContext _context;

    public RoutingOutboxDataAccess(TcmbDatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<PendingRoutingPayment>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_RoutingOutbox_Pending");
        command.Parameters.Add("@BatchSize", SqlDbType.Int).Value = batchSize;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var messages = new List<PendingRoutingPayment>();
        while (await reader.ReadAsync(cancellationToken))
        {
            messages.Add(new PendingRoutingPayment
            {
                RoutingOutboxMessageId = reader.GetInt32(
                    reader.GetOrdinal("RoutingOutboxMessageId")),
                AttemptCount = reader.GetInt32(reader.GetOrdinal("AttemptCount")),
                PaymentOrderId = reader.GetInt32(reader.GetOrdinal("PaymentOrderId")),
                CentralReference = reader.GetString(reader.GetOrdinal("CentralReference")),
                SenderBankCode = reader.GetString(reader.GetOrdinal("SenderBankCode")),
                ReceiverBankCode = reader.GetString(reader.GetOrdinal("ReceiverBankCode")),
                ReceiverIban = reader.GetString(reader.GetOrdinal("ReceiverIban")),
                ReceiverName = reader.GetString(reader.GetOrdinal("ReceiverName")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Description = GetNullableString(reader, "Description"),
                ReceiverApiBaseUrl = reader.GetString(
                    reader.GetOrdinal("ReceiverApiBaseUrl"))
            });
        }

        return messages;
    }

    public async Task MarkRoutingAsync(
        int routingOutboxMessageId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_RoutingOutbox_MarkRouting");
        command.Parameters.Add("@RoutingOutboxMessageId", SqlDbType.Int).Value =
            routingOutboxMessageId;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkResultAsync(
        int routingOutboxMessageId,
        RoutePaymentResponse response,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_RoutingOutbox_MarkResult");
        command.Parameters.Add("@RoutingOutboxMessageId", SqlDbType.Int).Value =
            routingOutboxMessageId;
        command.Parameters.Add("@ResultStatus", SqlDbType.NVarChar, 30).Value =
            response.Status;
        command.Parameters.Add("@FailureReason", SqlDbType.NVarChar, 500).Value =
            (object?)response.FailureReason ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        int routingOutboxMessageId,
        string error,
        int maxAttempts,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCommand(connection, "sp_RoutingOutbox_MarkFailed");
        command.Parameters.Add("@RoutingOutboxMessageId", SqlDbType.Int).Value =
            routingOutboxMessageId;
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
