using System.Data;
using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class AccountDataAccess : IAccountDataAccess
{
    private readonly DatabaseContext _context;

    public AccountDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<AccountListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Accounts_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var accounts = new List<AccountListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            accounts.Add(MapAccountList(reader));
        }

        return accounts;
    }

    public async Task<AccountSelectDto?> SelectAsync(int accountId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Accounts_Select");

        AddAccountSelectParameters(command, accountId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapAccountSelect(reader) : null;
    }

    public async Task<int> InsertAsync(AccountCreateDto account, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Accounts_Insert");

        AddAccountCreateParameters(command, account);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(AccountUpdateDto account, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Accounts_Update");

        AddAccountUpdateParameters(command, account);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int accountId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Accounts_Delete");

        AddAccountDeleteParameters(command, accountId);

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

    private static void AddAccountCreateParameters(SqlCommand command, AccountCreateDto account)
    {
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = account.CustomerId;
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = account.BranchId;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = account.CurrencyCode;
        command.Parameters.Add("@Balance", SqlDbType.Decimal).Value = account.Balance;
    }

    private static void AddAccountUpdateParameters(SqlCommand command, AccountUpdateDto account)
    {
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = account.AccountId;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = account.CustomerId;
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = account.BranchId;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = account.CurrencyCode;
        command.Parameters.Add("@Balance", SqlDbType.Decimal).Value = account.Balance;
    }

    private static void AddAccountDeleteParameters(SqlCommand command, int accountId)
    {
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
    }

    private static void AddAccountSelectParameters(SqlCommand command, int accountId)
    {
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
    }

    private static AccountListDto MapAccountList(SqlDataReader reader)
    {
        return new AccountListDto
        {
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    private static AccountSelectDto MapAccountSelect(SqlDataReader reader)
    {
        return new AccountSelectDto
        {
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
            CreatedDate = GetOptionalDateTime(reader, "CreatedDate"),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    public async Task TransferBetweenAsync(AccountTransferDto dto, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Account_TransferBetween");

        command.Parameters.Add("@SourceAccountId", SqlDbType.Int).Value = dto.SourceAccountId;
        command.Parameters.Add("@TargetAccountId", SqlDbType.Int).Value = dto.TargetAccountId;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = dto.Amount;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)dto.Description ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DateTime GetOptionalDateTime(SqlDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return reader.IsDBNull(i) ? default : reader.GetDateTime(i);
            }
        }

        return default;
    }
}
