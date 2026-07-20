using System.Data;
using BankApp.BankApp.Common.Dtos.Loan;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class LoanDataAccess : ILoanDataAccess
{
    private readonly DatabaseContext _context;

    public LoanDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<LoanTypeDto>> GetLoanTypesAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_LoanTypes_List", conn) { CommandType = CommandType.StoredProcedure };
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<LoanTypeDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new LoanTypeDto
            {
                LoanTypeId = reader.GetInt32(reader.GetOrdinal("LoanTypeId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                AnnualInterestRate = reader.GetDecimal(reader.GetOrdinal("AnnualInterestRate")),
                MinAmount = reader.GetDecimal(reader.GetOrdinal("MinAmount")),
                MaxAmount = reader.GetDecimal(reader.GetOrdinal("MaxAmount")),
                MinTermMonths = reader.GetInt32(reader.GetOrdinal("MinTermMonths")),
                MaxTermMonths = reader.GetInt32(reader.GetOrdinal("MaxTermMonths"))
            });
        }
        return list;
    }

    public async Task<List<LoanListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_List", conn) { CommandType = CommandType.StoredProcedure };
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<LoanListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapLoanList(reader));
        }
        return list;
    }

    public async Task<LoanDetailDto?> SelectAsync(int loanId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_Select", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new LoanDetailDto
            {
                LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerFirstName = GetNullableString(reader, "CustomerFirstName"),
                CustomerLastName = GetNullableString(reader, "CustomerLastName"),
                LoanTypeName = GetNullableString(reader, "LoanTypeName"),
                LoanTypeId = reader.GetInt32(reader.GetOrdinal("LoanTypeId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TermMonths = reader.GetInt32(reader.GetOrdinal("TermMonths")),
                AnnualInterestRate = reader.GetDecimal(reader.GetOrdinal("AnnualInterestRate")),
                MonthlyPayment = reader.GetDecimal(reader.GetOrdinal("MonthlyPayment")),
                DisbursementAccountId = reader.GetInt32(reader.GetOrdinal("DisbursementAccountId")),
                PaymentAccountId = reader.GetInt32(reader.GetOrdinal("PaymentAccountId")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
                ApprovedAt = GetNullableDateTime(reader, "ApprovedAt"),
                ClosedAt = GetNullableDateTime(reader, "ClosedAt"),
                PaymentsMade = reader.GetInt32(reader.GetOrdinal("PaymentsMade")),
                PaymentsMissed = reader.GetInt32(reader.GetOrdinal("PaymentsMissed")),
                RemainingPrincipal = reader.GetDecimal(reader.GetOrdinal("RemainingPrincipal"))
            };
        }
        return null;
    }

    public async Task ApproveAsync(int loanId, decimal monthlyPayment, List<LoanScheduleDto> schedules, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        using var tx = conn.BeginTransaction();

        try
        {
            using var cmd1 = new SqlCommand(@"
                DECLARE @Amt decimal(18,2),@Dbal decimal(18,2),@DisbAcc int,@CustId int;
                SELECT @Amt=l.Amount,@DisbAcc=l.DisbursementAccountId,@CustId=l.CustomerId
                FROM Loans l WITH(UPDLOCK,HOLDLOCK)
                WHERE l.LoanId=@LoanId AND l.Status='Pending';
                IF @Amt IS NULL THROW 50000, 'Loan not found or already processed.', 1;
                SELECT @Dbal=Balance FROM Accounts WHERE AccountId=@DisbAcc;
                UPDATE Accounts SET Balance=Balance+@Amt WHERE AccountId=@DisbAcc;
                UPDATE Loans SET Status='Active',ApprovedAt=GETDATE(),MonthlyPayment=@MonthlyPayment,RemainingPrincipal=@Amt WHERE LoanId=@LoanId;
            ", conn, tx);
            cmd1.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
            cmd1.Parameters.Add("@MonthlyPayment", SqlDbType.Decimal).Value = monthlyPayment;
            await cmd1.ExecuteNonQueryAsync(cancellationToken);

            foreach (var s in schedules)
            {
                using var cmd2 = new SqlCommand(@"
                    INSERT INTO LoanSchedules(LoanId,PeriodNumber,DueDate,Principal,Interest,TotalDue,RemainingBalance)
                    VALUES(@LoanId,@Period,@Due,@Principal,@Interest,@Total,@Remaining);
                ", conn, tx);
                cmd2.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
                cmd2.Parameters.Add("@Period", SqlDbType.Int).Value = s.PeriodNumber;
                cmd2.Parameters.Add("@Due", SqlDbType.DateTime2).Value = s.DueDate;
                cmd2.Parameters.Add("@Principal", SqlDbType.Decimal).Value = s.Principal;
                cmd2.Parameters.Add("@Interest", SqlDbType.Decimal).Value = s.Interest;
                cmd2.Parameters.Add("@Total", SqlDbType.Decimal).Value = s.TotalDue;
                cmd2.Parameters.Add("@Remaining", SqlDbType.Decimal).Value = s.RemainingBalance;
                await cmd2.ExecuteNonQueryAsync(cancellationToken);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task RejectAsync(int loanId, string? reason, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_Reject", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        cmd.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = 0;
        cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = (object?)reason ?? DBNull.Value;
        await conn.OpenAsync(cancellationToken);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<LoanScheduleDto>> GetScheduleAsync(int loanId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_GetSchedule", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<LoanScheduleDto>();
        while (await reader.ReadAsync(cancellationToken))
            list.Add(MapSchedule(reader));
        return list;
    }

    public async Task<List<LoanPaymentDto>> GetPaymentsAsync(int loanId, CancellationToken cancellationToken = default)
    {
        using var conn = _context.CreateConnection();
        using var cmd = new SqlCommand("sp_Loans_GetPayments", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
        await conn.OpenAsync(cancellationToken);
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<LoanPaymentDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new LoanPaymentDto
            {
                PaymentId = reader.GetInt32(reader.GetOrdinal("PaymentId")),
                ScheduleId = reader.IsDBNull(reader.GetOrdinal("ScheduleId")) ? null : reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                PaymentType = reader.GetString(reader.GetOrdinal("PaymentType")),
                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                Description = GetNullableString(reader, "Description")
            });
        }
        return list;
    }

    private static LoanListDto MapLoanList(SqlDataReader reader)
    {
        return new LoanListDto
        {
            LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            CustomerFirstName = GetNullableString(reader, "CustomerFirstName"),
            CustomerLastName = GetNullableString(reader, "CustomerLastName"),
            LoanTypeName = GetNullableString(reader, "LoanTypeName"),
            LoanTypeId = reader.GetInt32(reader.GetOrdinal("LoanTypeId")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            TermMonths = reader.GetInt32(reader.GetOrdinal("TermMonths")),
            AnnualInterestRate = reader.GetDecimal(reader.GetOrdinal("AnnualInterestRate")),
            MonthlyPayment = reader.GetDecimal(reader.GetOrdinal("MonthlyPayment")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            AppliedAt = reader.GetDateTime(reader.GetOrdinal("AppliedAt")),
            ApprovedAt = GetNullableDateTime(reader, "ApprovedAt"),
            PaymentsMade = reader.GetInt32(reader.GetOrdinal("PaymentsMade")),
            PaymentsMissed = reader.GetInt32(reader.GetOrdinal("PaymentsMissed")),
            RemainingPrincipal = reader.GetDecimal(reader.GetOrdinal("RemainingPrincipal"))
        };
    }

    private static LoanScheduleDto MapSchedule(SqlDataReader reader)
    {
        return new LoanScheduleDto
        {
            ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
            LoanId = reader.GetInt32(reader.GetOrdinal("LoanId")),
            PeriodNumber = reader.GetInt32(reader.GetOrdinal("PeriodNumber")),
            DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
            Principal = reader.GetDecimal(reader.GetOrdinal("Principal")),
            Interest = reader.GetDecimal(reader.GetOrdinal("Interest")),
            TotalDue = reader.GetDecimal(reader.GetOrdinal("TotalDue")),
            RemainingBalance = reader.GetDecimal(reader.GetOrdinal("RemainingBalance")),
            IsPaid = reader.GetBoolean(reader.GetOrdinal("IsPaid")),
            PaidDate = GetNullableDateTime(reader, "PaidDate"),
            IsLate = reader.GetBoolean(reader.GetOrdinal("IsLate"))
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string col)
    {
        var ord = reader.GetOrdinal(col);
        return reader.IsDBNull(ord) ? null : reader.GetString(ord);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string col)
    {
        var ord = reader.GetOrdinal(col);
        return reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);
    }
}
