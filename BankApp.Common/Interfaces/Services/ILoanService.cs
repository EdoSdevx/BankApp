using BankApp.BankApp.Common.Dtos.Loan;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface ILoanService
{
    Task<Result<List<LoanTypeDto>>> GetLoanTypesAsync(CancellationToken cancellationToken = default);
    Task<Result<List<LoanListDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<LoanDetailDto>> SelectAsync(int loanId, CancellationToken cancellationToken = default);
    Task<Result> ApproveAsync(int loanId, int employeeId, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(int loanId, string? reason, CancellationToken cancellationToken = default);
    Task<Result<List<LoanScheduleDto>>> GetScheduleAsync(int loanId, CancellationToken cancellationToken = default);
    Task<Result<List<LoanPaymentDto>>> GetPaymentsAsync(int loanId, CancellationToken cancellationToken = default);
}
