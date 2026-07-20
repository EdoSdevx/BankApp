using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.Services;

public class CustomerLoanService : ICustomerLoanService
{
    private readonly ICustomerLoanDataAccess _dataAccess;

    public CustomerLoanService(ICustomerLoanDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public async Task<Result<List<LoanListDto>>> GetMyLoansAsync(int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _dataAccess.GetMyLoansAsync(customerId, cancellationToken);
            return Result<List<LoanListDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            return Result<List<LoanListDto>>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> ApplyAsync(LoanApplyDto dto, int customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.Amount <= 0)
                return Result.Fail("Amount must be greater than zero.");

            if (dto.TermMonths <= 0)
                return Result.Fail("Term must be greater than zero.");

            if (dto.DisbursementAccountId <= 0)
                return Result.Fail("Disbursement account is required.");

            if (dto.PaymentAccountId <= 0)
                return Result.Fail("Payment account is required.");

            await _dataAccess.ApplyAsync(dto, customerId, cancellationToken);

            return Result.Ok("Loan application submitted.");
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

    public async Task<Result> MakePaymentAsync(int customerId, int loanId, int scheduleId, int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (loanId <= 0 || scheduleId <= 0 || accountId <= 0)
                return Result.Fail("Invalid parameters.");

            await _dataAccess.MakePaymentAsync(loanId, scheduleId, accountId, cancellationToken);

            return Result.Ok("Payment made successfully.");
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

    public async Task<Result> CloseEarlyAsync(int customerId, int loanId, int accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (loanId <= 0 || accountId <= 0)
                return Result.Fail("Invalid parameters.");

            await _dataAccess.CloseEarlyAsync(loanId, accountId, cancellationToken);

            return Result.Ok("Loan closed early.");
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
