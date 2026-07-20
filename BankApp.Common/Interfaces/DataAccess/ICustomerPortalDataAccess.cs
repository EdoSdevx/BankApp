using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Dtos.Transactions;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ICustomerPortalDataAccess
{
    Task<CustomerDashboardDto> GetDashboardAsync(int customerId, CancellationToken cancellationToken = default);
    Task<List<AccountListDto>> GetAccountsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<AccountSelectDto?> GetAccountAsync(int accountId, int customerId, CancellationToken cancellationToken = default);
    Task<int> CreateAccountAsync(int customerId, int branchId, string currencyCode, CancellationToken cancellationToken = default);
    Task<List<TransactionListDto>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<List<BillListDto>> GetBillsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<TransferResultDto> TransferAsync(int customerId, int sourceAccountId, int targetAccountId, decimal amount, string? description, CancellationToken cancellationToken = default);
    Task PayBillAsync(int customerId, int billId, int? accountId = null, CancellationToken cancellationToken = default);
    Task ExchangeAsync(int customerId, int sourceAccountId, int targetAccountId, decimal targetAmount, CancellationToken cancellationToken = default);
    Task<AccountOwnerDto?> LookupOwnerAsync(int accountId, CancellationToken cancellationToken = default);
    Task<List<RecentTransferDto>> GetRecentTransfersAsync(int accountId, CancellationToken cancellationToken = default);
    Task<List<BranchListDto>> GetBranchesAsync(CancellationToken cancellationToken = default);
    Task<List<CurrencyListDto>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<List<ExchangeRateListDto>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task TransferBetweenAsync(AccountTransferDto dto, CancellationToken cancellationToken = default);
}
