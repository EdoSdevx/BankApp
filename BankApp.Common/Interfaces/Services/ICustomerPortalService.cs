using BankApp.BankApp.Common.Dtos.Accounts;
using BankApp.BankApp.Common.Dtos.Bills;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Dtos.Customer;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Dtos.Transactions;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ICustomerPortalService
{
    Task<Result<CustomerDashboardDto>> GetDashboardAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result<List<AccountListDto>>> GetAccountsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result<AccountSelectDto>> GetAccountAsync(int accountId, int customerId, CancellationToken cancellationToken = default);
    Task<Result> CreateAccountAsync(int customerId, int branchId, string currencyCode, CancellationToken cancellationToken = default);
    Task<Result<List<TransactionListDto>>> GetTransactionsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result<List<BillListDto>>> GetBillsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result> TransferAsync(int customerId, TransferRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result> PayBillAsync(int customerId, int billId, int? accountId = null, CancellationToken cancellationToken = default);
    Task<Result> ExchangeAsync(int customerId, ExchangeRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result<AccountOwnerDto>> LookupOwnerAsync(int accountId, CancellationToken cancellationToken = default);
    Task<Result<List<RecentTransferDto>>> GetRecentTransfersAsync(int accountId, CancellationToken cancellationToken = default);
    Task<Result<List<BranchListDto>>> GetBranchesAsync(CancellationToken cancellationToken = default);
    Task<Result<List<CurrencyListDto>>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<Result<List<ExchangeRateListDto>>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task<Result> TransferBetweenAsync(int customerId, AccountTransferDto dto, CancellationToken cancellationToken = default);
}
