using BankApp.BankApp.Common.Dtos.Loan;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ICustomerLoanDataAccess
{
    Task<List<LoanListDto>> GetMyLoansAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> ApplyAsync(LoanApplyDto dto, int customerId, CancellationToken cancellationToken = default);
    Task MakePaymentAsync(int loanId, int scheduleId, int accountId, CancellationToken cancellationToken = default);
    Task CloseEarlyAsync(int loanId, int accountId, CancellationToken cancellationToken = default);
}
