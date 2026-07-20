using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Dtos.Transactions;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class CustomerPortalService : ICustomerPortalService
{
    private readonly ICustomerPortalDataAccess _dataAccess;
    private readonly IHubContext<NotificationHub> _hubContext;

    public CustomerPortalService(ICustomerPortalDataAccess dataAccess, IHubContext<NotificationHub> hubContext)
    {
        _dataAccess = dataAccess;
        _hubContext = hubContext;
    }

    public async Task<Result<CustomerDashboardDto>> GetDashboardAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _dataAccess.GetDashboardAsync(customerId, cancellationToken);
            return Result<CustomerDashboardDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Result<CustomerDashboardDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<AccountListDto>>> GetAccountsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _dataAccess.GetAccountsAsync(customerId, cancellationToken);
            return Result<List<AccountListDto>>.Ok(accounts);
        }
        catch (Exception ex)
        {
            return Result<List<AccountListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<AccountSelectDto>> GetAccountAsync(int accountId, int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _dataAccess.GetAccountAsync(accountId, customerId, cancellationToken);

            if (account is null)
            {
                return Result<AccountSelectDto>.NotFound("Account not found.");
            }

            return Result<AccountSelectDto>.Ok(account);
        }
        catch (Exception ex)
        {
            return Result<AccountSelectDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> CreateAccountAsync(int customerId, int branchId, string currencyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (branchId <= 0)
            {
                return Result.Fail("Branch ID must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Trim().Length != 3)
            {
                return Result.Fail("Currency code must be 3 characters.");
            }

            currencyCode = currencyCode.Trim().ToUpperInvariant();
            var accountId = await _dataAccess.CreateAccountAsync(customerId, branchId, currencyCode, cancellationToken);

            return Result.Ok("Account created successfully.");
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return Result.Conflict("An account with this ID already exists.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<TransactionListDto>>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _dataAccess.GetTransactionsAsync(customerId, cancellationToken);
            return Result<List<TransactionListDto>>.Ok(transactions);
        }
        catch (Exception ex)
        {
            return Result<List<TransactionListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<BillListDto>>> GetBillsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var bills = await _dataAccess.GetBillsAsync(customerId, cancellationToken);
            return Result<List<BillListDto>>.Ok(bills);
        }
        catch (Exception ex)
        {
            return Result<List<BillListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> TransferAsync(int customerId, TransferRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.SourceAccountId <= 0)
                return Result.Fail("Source account ID is required.");

            if (dto.TargetAccountId <= 0)
                return Result.Fail("Target account ID is required.");

            if (dto.SourceAccountId == dto.TargetAccountId)
                return Result.Fail("Source and target accounts must be different.");

            if (dto.Amount <= 0)
                return Result.Fail("Amount must be greater than zero.");

            var transferResult = await _dataAccess.TransferAsync(
                customerId,
                dto.SourceAccountId,
                dto.TargetAccountId,
                dto.Amount,
                dto.Description,
                cancellationToken);

            if (transferResult.TransferStatus == "Pending")
            {
                await _hubContext.Clients.Group("Admins").SendAsync("NewPendingTransfer", new
                {
                    transferResult.PendingTransferId,
                    Amount = dto.Amount,
                    dto.SourceAccountId,
                    dto.TargetAccountId,
                    Description = dto.Description
                }, cancellationToken);
            }

            return Result.Ok("Transfer completed successfully.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> PayBillAsync(int customerId, int billId, int? accountId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (billId <= 0)
                return Result.Fail("Bill ID must be greater than zero.");

            await _dataAccess.PayBillAsync(customerId, billId, accountId, cancellationToken);

            return Result.Ok("Bill paid successfully.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> ExchangeAsync(int customerId, ExchangeRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.SourceAccountId <= 0)
                return Result.Fail("Source account ID is required.");

            if (dto.TargetAccountId <= 0)
                return Result.Fail("Target account ID is required.");

            if (dto.SourceAccountId == dto.TargetAccountId)
                return Result.Fail("Source and target accounts must be different.");

            if (dto.TargetAmount <= 0)
                return Result.Fail("Amount must be greater than zero.");

            await _dataAccess.ExchangeAsync(
                customerId,
                dto.SourceAccountId,
                dto.TargetAccountId,
                dto.TargetAmount,
                cancellationToken);

            return Result.Ok("Exchange completed successfully.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<AccountOwnerDto>> LookupOwnerAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (accountId <= 0)
                return Result<AccountOwnerDto>.Fail("Account ID must be greater than zero.");

            var owner = await _dataAccess.LookupOwnerAsync(accountId, cancellationToken);

            if (owner is null)
                return Result<AccountOwnerDto>.NotFound("Account not found.");

            return Result<AccountOwnerDto>.Ok(owner);
        }
        catch (Exception ex)
        {
            return Result<AccountOwnerDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<RecentTransferDto>>> GetRecentTransfersAsync(int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (accountId <= 0)
                return Result<List<RecentTransferDto>>.Fail("Account ID must be greater than zero.");

            var transfers = await _dataAccess.GetRecentTransfersAsync(accountId, cancellationToken);
            return Result<List<RecentTransferDto>>.Ok(transfers);
        }
        catch (Exception ex)
        {
            return Result<List<RecentTransferDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<BranchListDto>>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var branches = await _dataAccess.GetBranchesAsync(cancellationToken);
            return Result<List<BranchListDto>>.Ok(branches);
        }
        catch (Exception ex)
        {
            return Result<List<BranchListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<CurrencyListDto>>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currencies = await _dataAccess.GetCurrenciesAsync(cancellationToken);
            return Result<List<CurrencyListDto>>.Ok(currencies);
        }
        catch (Exception ex)
        {
            return Result<List<CurrencyListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<ExchangeRateListDto>>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rates = await _dataAccess.GetExchangeRatesAsync(cancellationToken);
            return Result<List<ExchangeRateListDto>>.Ok(rates);
        }
        catch (Exception ex)
        {
            return Result<List<ExchangeRateListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> TransferBetweenAsync(int customerId, AccountTransferDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.SourceAccountId <= 0)
                return Result.Fail("Source account ID must be greater than zero.");

            if (dto.TargetAccountId <= 0)
                return Result.Fail("Target account ID must be greater than zero.");

            if (dto.SourceAccountId == dto.TargetAccountId)
                return Result.Fail("Source and target accounts must be different.");

            if (dto.Amount <= 0)
                return Result.Fail("Amount must be greater than zero.");

            var sourceAccount = await _dataAccess.GetAccountAsync(dto.SourceAccountId, customerId, cancellationToken);
            if (sourceAccount is null)
                return Result.Fail("Source account does not belong to you.");

            await _dataAccess.TransferBetweenAsync(dto, cancellationToken);

            return Result.Ok("Transfer completed successfully.");
        }
        catch (SqlException ex)
        {
            return Result.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }
}
