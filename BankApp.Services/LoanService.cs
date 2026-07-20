using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class LoanService : ILoanService
{
    private readonly ILoanDataAccess _dataAccess;

    public LoanService(ILoanDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<LoanTypeDto>>> GetLoanTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _dataAccess.GetLoanTypesAsync(cancellationToken);
            return Result<List<LoanTypeDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            return Result<List<LoanTypeDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<LoanListDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _dataAccess.ListAsync(cancellationToken);
            return Result<List<LoanListDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            return Result<List<LoanListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<LoanDetailDto>> SelectAsync(int loanId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (loanId <= 0)
                return Result<LoanDetailDto>.Fail("Loan ID must be greater than zero.");

            var loan = await _dataAccess.SelectAsync(loanId, cancellationToken);

            if (loan is null)
                return Result<LoanDetailDto>.NotFound("Loan not found.");

            return Result<LoanDetailDto>.Ok(loan);
        }
        catch (Exception ex)
        {
            return Result<LoanDetailDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> ApproveAsync(int loanId, int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var loan = await _dataAccess.SelectAsync(loanId, cancellationToken);

            if (loan is null)
                return Result.NotFound("Loan not found.");

            if (loan.Status != "Pending")
                return Result.Fail("Loan is not in pending status.");

            var schedules = GenerateSchedule(loan.Amount, loan.AnnualInterestRate, loan.TermMonths);

            await _dataAccess.ApproveAsync(loanId, schedules[0].TotalDue, schedules, cancellationToken);

            return Result.Ok("Loan approved and disbursed.");
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

    public async Task<Result> RejectAsync(int loanId, string? reason, CancellationToken cancellationToken = default)
    {
        try
        {
            if (loanId <= 0)
                return Result.Fail("Loan ID must be greater than zero.");

            await _dataAccess.RejectAsync(loanId, reason, cancellationToken);

            return Result.Ok("Loan rejected.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<LoanScheduleDto>>> GetScheduleAsync(int loanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _dataAccess.GetScheduleAsync(loanId, cancellationToken);
            return Result<List<LoanScheduleDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            return Result<List<LoanScheduleDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result<List<LoanPaymentDto>>> GetPaymentsAsync(int loanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _dataAccess.GetPaymentsAsync(loanId, cancellationToken);
            return Result<List<LoanPaymentDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            return Result<List<LoanPaymentDto>>.DatabaseError(ex.Message);
        }
    }

    public static List<LoanScheduleDto> GenerateSchedule(decimal principal, decimal annualRate, int termMonths)
    {
        var schedule = new List<LoanScheduleDto>();
        var monthlyRate = annualRate / 12;
        var power = (decimal)Math.Pow((double)(1 + monthlyRate), termMonths);
        var monthlyPayment = principal * monthlyRate * power / (power - 1);
        var remaining = principal;
        var now = DateTime.UtcNow;

        for (int i = 1; i <= termMonths; i++)
        {
            var interest = Math.Round(remaining * monthlyRate, 2);
            var principalPortion = Math.Round(monthlyPayment - interest, 2);
            if (i == termMonths) principalPortion = remaining;
            remaining = Math.Round(remaining - principalPortion, 2);
            var totalDue = Math.Round(principalPortion + interest, 2);

            schedule.Add(new LoanScheduleDto
            {
                LoanId = 0,
                PeriodNumber = i,
                DueDate = now.AddSeconds(i * 10),
                Principal = principalPortion,
                Interest = interest,
                TotalDue = totalDue,
                RemainingBalance = Math.Max(0, remaining),
                IsPaid = false,
                IsLate = false
            });
        }

        return schedule;
    }
}
