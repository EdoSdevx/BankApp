using System.Data;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class BillDataAccess : IBillDataAccess
{
    private readonly DatabaseContext _context;

    public BillDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<BillListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var bills = new List<BillListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            bills.Add(MapBillList(reader));
        }

        return bills;
    }

    public async Task<BillSelectDto?> SelectAsync(int billId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_Select");

        AddBillSelectParameters(command, billId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapBillSelect(reader) : null;
    }

    public async Task<int> InsertAsync(BillCreateDto bill, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_Insert");

        AddBillCreateParameters(command, bill);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(BillUpdateDto bill, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_Update");

        AddBillUpdateParameters(command, bill);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> MarkPaidAsync(int billId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_MarkPaid");

        command.Parameters.Add("@BillId", SqlDbType.Int).Value = billId;

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int billId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Bills_Delete");

        AddBillDeleteParameters(command, billId);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SqlCommand CreateStoredProcedureCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static void AddBillCreateParameters(SqlCommand command, BillCreateDto bill)
    {
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = bill.CustomerId;
        command.Parameters.Add("@BillType", SqlDbType.NVarChar, 255).Value = bill.BillType;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = bill.Amount;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = (object?)bill.CurrencyCode ?? DBNull.Value;
        command.Parameters.Add("@DueDate", SqlDbType.DateTime2).Value = bill.DueDate;
        command.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = bill.IsPaid;
        command.Parameters.Add("@PaidDate", SqlDbType.DateTime2).Value = (object?)bill.PaidDate ?? DBNull.Value;
    }

    private static void AddBillUpdateParameters(SqlCommand command, BillUpdateDto bill)
    {
        command.Parameters.Add("@BillId", SqlDbType.Int).Value = bill.BillId;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = bill.CustomerId;
        command.Parameters.Add("@BillType", SqlDbType.NVarChar, 255).Value = bill.BillType;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = bill.Amount;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = (object?)bill.CurrencyCode ?? DBNull.Value;
        command.Parameters.Add("@DueDate", SqlDbType.DateTime2).Value = bill.DueDate;
        command.Parameters.Add("@IsPaid", SqlDbType.Bit).Value = bill.IsPaid;
        command.Parameters.Add("@PaidDate", SqlDbType.DateTime2).Value = (object?)bill.PaidDate ?? DBNull.Value;
    }

    private static void AddBillDeleteParameters(SqlCommand command, int billId)
    {
        command.Parameters.Add("@BillId", SqlDbType.Int).Value = billId;
    }

    private static void AddBillSelectParameters(SqlCommand command, int billId)
    {
        command.Parameters.Add("@BillId", SqlDbType.Int).Value = billId;
    }

    private static BillListDto MapBillList(SqlDataReader reader)
    {
        return new BillListDto
        {
            BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            BillType = reader.GetString(reader.GetOrdinal("BillType")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            CurrencyCode = GetNullableString(reader, "CurrencyCode"),
            DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
            IsPaid = reader.GetBoolean(reader.GetOrdinal("IsPaid")),
            PaidDate = GetNullableDateTime(reader, "PaidDate")
        };
    }

    private static BillSelectDto MapBillSelect(SqlDataReader reader)
    {
        return new BillSelectDto
        {
            BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            BillType = reader.GetString(reader.GetOrdinal("BillType")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            CurrencyCode = GetNullableString(reader, "CurrencyCode"),
            DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
            IsPaid = reader.GetBoolean(reader.GetOrdinal("IsPaid")),
            PaidDate = GetNullableDateTime(reader, "PaidDate")
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
