using BankApp.BankApp.Common.Dtos.Loan;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface ILoanDataAccess
{
    Task<List<LoanTypeDto>> GetLoanTypesAsync(CancellationToken cancellationToken = default);
    Task<List<LoanListDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<LoanDetailDto?> SelectAsync(int loanId, CancellationToken cancellationToken = default);
    Task ApproveAsync(int loanId, decimal monthlyPayment, List<LoanScheduleDto> schedules, CancellationToken cancellationToken = default);
    Task RejectAsync(int loanId, string? reason, CancellationToken cancellationToken = default);
    Task<List<LoanScheduleDto>> GetScheduleAsync(int loanId, CancellationToken cancellationToken = default);
    Task<List<LoanPaymentDto>> GetPaymentsAsync(int loanId, CancellationToken cancellationToken = default);
}
