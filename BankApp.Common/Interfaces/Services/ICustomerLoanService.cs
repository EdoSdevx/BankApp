using BankApp.BankApp.Common.Dtos.Loan;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ICustomerLoanService
{
    Task<Result<List<LoanListDto>>> GetMyLoansAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Result> ApplyAsync(LoanApplyDto dto, int customerId, CancellationToken cancellationToken = default);
    Task<Result> MakePaymentAsync(int customerId, int loanId, int scheduleId, int accountId, CancellationToken cancellationToken = default);
    Task<Result> CloseEarlyAsync(int customerId, int loanId, int accountId, CancellationToken cancellationToken = default);
}
