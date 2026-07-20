using System.Data;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class AdminApprovalDataAccess : IAdminApprovalDataAccess
{
    private readonly DatabaseContext _context;

    public AdminApprovalDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<PendingTransferDto>> GetPendingTransfersAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand("sp_Admin_PendingTransfers", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transfers = new List<PendingTransferDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transfers.Add(new PendingTransferDto
            {
                PendingTransferId = reader.GetInt32(reader.GetOrdinal("PendingTransferId")),
                SourceAccountId = reader.GetInt32(reader.GetOrdinal("SourceAccountId")),
                TargetAccountId = reader.GetInt32(reader.GetOrdinal("TargetAccountId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Description = GetNullableString(reader, "Description"),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedByCustomerId = reader.GetInt32(reader.GetOrdinal("CreatedByCustomerId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SrcFirstName = GetNullableString(reader, "SrcFirstName"),
                SrcLastName = GetNullableString(reader, "SrcLastName"),
                TgtFirstName = GetNullableString(reader, "TgtFirstName"),
                TgtLastName = GetNullableString(reader, "TgtLastName")
            });
        }

        return transfers;
    }

    public async Task<ApprovalResultDto> ApproveTransferAsync(int pendingTransferId, int employeeId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand("sp_ApproveTransfer", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@PendingTransferId", SqlDbType.Int).Value = pendingTransferId;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new ApprovalResultDto
            {
                CreatedByCustomerId = reader.GetInt32(reader.GetOrdinal("CreatedByCustomerId"))
            };
        }

        throw new InvalidOperationException("No result from approve stored procedure.");
    }

    public async Task<ApprovalResultDto> RejectTransferAsync(int pendingTransferId, int employeeId, string? reason, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand("sp_RejectTransfer", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add("@PendingTransferId", SqlDbType.Int).Value = pendingTransferId;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
        command.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = (object?)reason ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new ApprovalResultDto
            {
                CreatedByCustomerId = reader.GetInt32(reader.GetOrdinal("CreatedByCustomerId"))
            };
        }

        throw new InvalidOperationException("No result from reject stored procedure.");
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
