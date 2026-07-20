using System.Data;
using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Dtos.Transactions;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class CustomerPortalDataAccess : ICustomerPortalDataAccess
{
    private readonly DatabaseContext _context;

    public CustomerPortalDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CustomerDashboardDto> GetDashboardAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var dto = new CustomerDashboardDto();

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Accounts WHERE CustomerId = @CustomerId", connection))
        {
            cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
            dto.AccountCount = (int)await cmd.ExecuteScalarAsync(cancellationToken)!;
        }

        using (var cmd = new SqlCommand("SELECT ISNULL(SUM(Balance), 0) FROM Accounts WHERE CustomerId = @CustomerId", connection))
        {
            cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
            dto.TotalBalance = (decimal)await cmd.ExecuteScalarAsync(cancellationToken)!;
        }

        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Bills WHERE CustomerId = @CustomerId AND IsPaid = 0", connection))
        {
            cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
            dto.UnpaidBillCount = (int)await cmd.ExecuteScalarAsync(cancellationToken)!;
        }

        return dto;
    }

    public async Task<List<AccountListDto>> GetAccountsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_Accounts");
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var accounts = new List<AccountListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            accounts.Add(new AccountListDto
            {
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }

        return accounts;
    }

    public async Task<AccountSelectDto?> GetAccountAsync(int accountId, int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand(
            "SELECT * FROM Accounts WHERE AccountId = @AccountId AND CustomerId = @CustomerId", connection);

        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new AccountSelectDto
            {
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        return null;
    }

    public async Task<int> CreateAccountAsync(int customerId, int branchId, string currencyCode, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_CreateAccount");

        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currencyCode;

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<List<TransactionListDto>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_Transactions");
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transactions = new List<TransactionListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transactions.Add(new TransactionListDto
            {
                TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
            });
        }

        return transactions;
    }

    public async Task<List<BillListDto>> GetBillsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_Bills");
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var bills = new List<BillListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            bills.Add(new BillListDto
            {
                BillId = reader.GetInt32(reader.GetOrdinal("BillId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                BillType = reader.GetString(reader.GetOrdinal("BillType")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = GetNullableString(reader, "CurrencyCode"),
                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                IsPaid = reader.GetBoolean(reader.GetOrdinal("IsPaid")),
                PaidDate = GetNullableDateTime(reader, "PaidDate")
            });
        }

        return bills;
    }

    public async Task<TransferResultDto> TransferAsync(int customerId, int sourceAccountId, int targetAccountId, decimal amount, string? description, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_TransferWithHold");

        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@SourceAccountId", SqlDbType.Int).Value = sourceAccountId;
        command.Parameters.Add("@TargetAccountId", SqlDbType.Int).Value = targetAccountId;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)description ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new TransferResultDto
            {
                TransferStatus = reader.GetString(reader.GetOrdinal("TransferStatus")),
                PendingTransferId = reader.IsDBNull(reader.GetOrdinal("PendingTransferId")) ? null : reader.GetInt32(reader.GetOrdinal("PendingTransferId"))
            };
        }

        throw new InvalidOperationException("No result from transfer stored procedure.");
    }

    public async Task PayBillAsync(int customerId, int billId, int? accountId = null, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_PayBill");

        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@BillId", SqlDbType.Int).Value = billId;
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = (object?)accountId ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ExchangeAsync(int customerId, int sourceAccountId, int targetAccountId, decimal targetAmount, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customer_Exchange");

        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        command.Parameters.Add("@SourceAccountId", SqlDbType.Int).Value = sourceAccountId;
        command.Parameters.Add("@TargetAccountId", SqlDbType.Int).Value = targetAccountId;
        command.Parameters.Add("@TargetAmount", SqlDbType.Decimal).Value = targetAmount;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AccountOwnerDto?> LookupOwnerAsync(int accountId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Account_Lookup");
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new AccountOwnerDto
            {
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName"))
            };
        }

        return null;
    }

    public async Task<List<RecentTransferDto>> GetRecentTransfersAsync(int accountId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Account_RecentTransfers");
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transfers = new List<RecentTransferDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transfers.Add(new RecentTransferDto
            {
                TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                Description = GetNullableString(reader, "Description"),
                RelatedAccountId = reader.IsDBNull(reader.GetOrdinal("RelatedAccountId")) ? null : reader.GetInt32(reader.GetOrdinal("RelatedAccountId")),
                FirstName = GetNullableString(reader, "FirstName"),
                LastName = GetNullableString(reader, "LastName"),
                RelatedCurrencyCode = GetNullableString(reader, "RelatedCurrencyCode")
            });
        }

        return transfers;
    }

    public async Task<List<BranchListDto>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var branches = new List<BranchListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            branches.Add(new BranchListDto
            {
                BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
                BranchCode = reader.GetString(reader.GetOrdinal("BranchCode")),
                City = reader.GetString(reader.GetOrdinal("City"))
            });
        }

        return branches;
    }

    public async Task<List<CurrencyListDto>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var currencies = new List<CurrencyListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            currencies.Add(new CurrencyListDto
            {
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                CurrencyName = reader.GetString(reader.GetOrdinal("CurrencyName"))
            });
        }

        return currencies;
    }

    public async Task<List<ExchangeRateListDto>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rates = new List<ExchangeRateListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rates.Add(new ExchangeRateListDto
            {
                RateId = reader.GetInt32(reader.GetOrdinal("RateId")),
                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                Rate = reader.GetDecimal(reader.GetOrdinal("Rate")),
                RateDate = reader.GetDateTime(reader.GetOrdinal("RateDate")),
                Source = reader.GetString(reader.GetOrdinal("Source"))
            });
        }

        return rates;
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

    private static SqlCommand CreateStoredProcedureCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
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
