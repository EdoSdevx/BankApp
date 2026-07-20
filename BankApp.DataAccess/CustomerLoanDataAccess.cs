using System.Data;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class CustomerLoanDataAccess : ICustomerLoanDataAccess
{
    private readonly DatabaseContext _context;

    public CustomerLoanDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<LoanListDto>> GetMyLoansAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Customer_Loans", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<LoanListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new LoanListDto
            {
                LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
                CustomerId = customerId,
                LoanTypeName = reader.IsDBNull(reader.GetOrdinal("LoanTypeName")) ? null : reader.GetString(reader.GetOrdinal("LoanTypeName")),
                LoanTypeId = reader.GetInt32(reader.GetOrdinal("LoanTypeId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TermMonths = reader.GetInt32(reader.GetOrdinal("TermMonths")),
                AnnualInterestRate = reader.GetDecimal(reader.GetOrdinal("AnnualInterestRate")),
                MonthlyPayment = reader.GetDecimal(reader.GetOrdinal("MonthlyPayment")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
                ApprovedAt = reader.IsDBNull(reader.GetOrdinal("ApprovedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedAt")),
                PaymentsMade = reader.GetInt32(reader.GetOrdinal("PaymentsMade")),
                PaymentsMissed = reader.GetInt32(reader.GetOrdinal("PaymentsMissed")),
                RemainingPrincipal = reader.GetDecimal(reader.GetOrdinal("RemainingPrincipal"))
            });
        }
        return list;
    }

    public async Task<int> ApplyAsync(LoanApplyDto dto, int customerId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_Apply", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
        cmd.Parameters.Add("@LoanTypeId", SqlDbType.Int).Value = dto.LoanTypeId;
        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = dto.Amount;
        cmd.Parameters.Add("@TermMonths", SqlDbType.Int).Value = dto.TermMonths;
        cmd.Parameters.Add("@DisbursementAccountId", SqlDbType.Int).Value = dto.DisbursementAccountId;
        cmd.Parameters.Add("@PaymentAccountId", SqlDbType.Int).Value = dto.PaymentAccountId;
        await conn.OpenAsync(cancellationToken);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task MakePaymentAsync(int loanId, int scheduleId, int accountId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_MakePayment", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        cmd.Parameters.Add("@ScheduleId", SqlDbType.Int).Value = scheduleId;
        cmd.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
        await conn.OpenAsync(cancellationToken);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task CloseEarlyAsync(int loanId, int accountId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_CloseEarly", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        cmd.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
        await conn.OpenAsync(cancellationToken);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
