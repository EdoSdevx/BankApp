using System.Data;
using BankApp.BankApp.Common.Dtos.Eft;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class EftDataAccess : IEftDataAccess
{
    private readonly DatabaseContext _context;

    public EftDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        int customerId,
        CreateEftRequestDto dto,
        string senderReference,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_CreateEft");

        command.Parameters.Add("@RequestId", SqlDbType.UniqueIdentifier).Value = dto.RequestId;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@SourceAccountId", SqlDbType.Int).Value = dto.SourceAccountId;
        command.Parameters.Add("@ReceiverIban", SqlDbType.NVarChar, 34).Value = dto.ReceiverIban;
        command.Parameters.Add("@ReceiverName", SqlDbType.NVarChar, 200).Value = dto.ReceiverName;

        var amountParameter = command.Parameters.Add("@Amount", SqlDbType.Decimal);
        amountParameter.Precision = 18;
        amountParameter.Scale = 2;
        amountParameter.Value = dto.Amount;

        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value =
            (object?)dto.Description ?? DBNull.Value;
        command.Parameters.Add("@SenderReference", SqlDbType.VarChar, 64).Value = senderReference;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<EftDetailDto>> GetByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_Efts");
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transfers = new List<EftDetailDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transfers.Add(MapEft(reader));
        }

        return transfers;
    }

    public async Task<EftDetailDto?> GetByIdAsync(
        int customerId,
        int eftTransferId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_EftDetail");
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@EftTransferId", SqlDbType.Int).Value = eftTransferId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapEft(reader) : null;
    }

    public async Task<List<PendingEftDto>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Admin_PendingEfts");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transfers = new List<PendingEftDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transfers.Add(new PendingEftDto
            {
                EftTransferId = reader.GetInt32(reader.GetOrdinal("EftTransferId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                SourceAccountId = reader.GetInt32(reader.GetOrdinal("SourceAccountId")),
                CustomerFirstName = reader.GetString(reader.GetOrdinal("CustomerFirstName")),
                CustomerLastName = reader.GetString(reader.GetOrdinal("CustomerLastName")),
                ReceiverIban = reader.GetString(reader.GetOrdinal("ReceiverIban")),
                ReceiverName = reader.GetString(reader.GetOrdinal("ReceiverName")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Description = GetNullableString(reader, "Description"),
                SenderReference = reader.GetString(reader.GetOrdinal("SenderReference")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return transfers;
    }

    public async Task<EftDetailDto> ApproveAsync(
        int eftTransferId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Admin_ApproveEft");
        command.Parameters.Add("@EftTransferId", SqlDbType.Int).Value = eftTransferId;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The EFT approval procedure returned no result.");
        }

        return MapEft(reader);
    }

    public async Task<EftDetailDto> RejectAsync(
        int eftTransferId,
        int employeeId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Admin_RejectEft");
        command.Parameters.Add("@EftTransferId", SqlDbType.Int).Value = eftTransferId;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
        command.Parameters.Add("@Reason", SqlDbType.NVarChar, 500).Value = reason;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The EFT rejection procedure returned no result.");
        }

        return MapEft(reader);
    }

    private static SqlCommand CreateStoredProcedureCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static EftDetailDto MapEft(SqlDataReader reader)
    {
        return new EftDetailDto
        {
            EftTransferId = reader.GetInt32(reader.GetOrdinal("EftTransferId")),
            RequestId = reader.GetGuid(reader.GetOrdinal("RequestId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            SourceAccountId = reader.GetInt32(reader.GetOrdinal("SourceAccountId")),
            ReceiverIban = reader.GetString(reader.GetOrdinal("ReceiverIban")),
            ReceiverName = reader.GetString(reader.GetOrdinal("ReceiverName")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Description = GetNullableString(reader, "Description"),
            SenderReference = reader.GetString(reader.GetOrdinal("SenderReference")),
            CentralReference = GetNullableString(reader, "CentralReference"),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            ApprovedAt = GetNullableDateTime(reader, "ApprovedAt"),
            SubmittedAt = GetNullableDateTime(reader, "SubmittedAt"),
            CompletedAt = GetNullableDateTime(reader, "CompletedAt"),
            FailureReason = GetNullableString(reader, "FailureReason")
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
