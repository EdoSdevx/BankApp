using System.Data;
using BankApp.BankApp.DataAccess;
using BankApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.BankApp.Services;

public class MonthlyLoanProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyLoanProcessor> _logger;

    public MonthlyLoanProcessor(IServiceScopeFactory scopeFactory, ILogger<MonthlyLoanProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                using var conn = db.CreateConnection();
                await conn.OpenAsync(stoppingToken);

                using var cmd = new SqlCommand("sp_Loans_DueSchedules", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await cmd.ExecuteReaderAsync(stoppingToken);
                var dueSchedules = new List<(int ScheduleId, int LoanId, decimal TotalDue, int CustomerId, int PaymentAccountId, decimal AccountBalance, string PaymentCurrency)>();

                while (await reader.ReadAsync(stoppingToken))
                {
                    dueSchedules.Add((
                        reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                        reader.GetInt32(reader.GetOrdinal("LoanId")),
                        reader.GetDecimal(reader.GetOrdinal("TotalDue")),
                        reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        reader.IsDBNull(reader.GetOrdinal("PaymentAccountId")) ? 0 : reader.GetInt32(reader.GetOrdinal("PaymentAccountId")),
                        reader.IsDBNull(reader.GetOrdinal("PaymentBalance")) ? 0 : reader.GetDecimal(reader.GetOrdinal("PaymentBalance")),
                        reader.IsDBNull(reader.GetOrdinal("PaymentCurrency")) ? string.Empty : reader.GetString(reader.GetOrdinal("PaymentCurrency"))
                    ));
                }

                reader.Close();

                foreach (var (scheduleId, loanId, totalDue, customerId, paymentAccountId, accountBalance, paymentCurrency) in dueSchedules)
                {
                    var targetAccountId = paymentAccountId;
                    var paid = false;

                    async Task<bool> TryPay(int accountId)
                    {
                        try
                        {
                            using var payCmd = new SqlCommand("sp_Loans_MakePayment", conn)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            payCmd.Parameters.Add("@LoanId", SqlDbType.Int).Value = loanId;
                            payCmd.Parameters.Add("@ScheduleId", SqlDbType.Int).Value = scheduleId;
                            payCmd.Parameters.Add("@AccountId", SqlDbType.Int).Value = accountId;
                            await payCmd.ExecuteNonQueryAsync(stoppingToken);
                            _logger.LogInformation("Auto-debit: Loan {LoanId}, Schedule {ScheduleId}, Account {AccountId}", loanId, scheduleId, accountId);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Payment attempt failed for Loan {LoanId}, Account {AccountId}", loanId, accountId);
                            return false;
                        }
                    }

                    if (targetAccountId > 0)
                    {
                        paid = await TryPay(targetAccountId);
                    }

                    if (!paid)
                    {
                        using var findCmd = new SqlCommand(@"
                            SELECT TOP 1 AccountId FROM Accounts
                            WHERE CustomerId=@CustId AND IsActive=1 AND CurrencyCode = @CurrencyCode Balance>=@Amt
                            ORDER BY Balance DESC;", conn);
                        findCmd.Parameters.Add("@CustId", SqlDbType.Int).Value = customerId;
                        findCmd.Parameters.Add("@Amt", SqlDbType.Decimal).Value = totalDue;
                        findCmd.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = paymentCurrency;
                        var found = await findCmd.ExecuteScalarAsync(stoppingToken);
                        if (found is not null)
                        {
                            paid = await TryPay((int)found);
                        }
                    }

                    if (!paid)
                    {
                        using var lateCmd = new SqlCommand(@"
                            UPDATE LoanSchedules SET IsLate=1 WHERE ScheduleId=@Sid;
                            UPDATE Loans SET PaymentsMissed=PaymentsMissed+1 WHERE LoanId=@Lid;
                            IF (SELECT PaymentsMissed FROM Loans WHERE LoanId=@Lid) >= 3
                                UPDATE Loans SET Status='Defaulted' WHERE LoanId=@Lid;
                        ", conn);
                        lateCmd.Parameters.Add("@Sid", SqlDbType.Int).Value = scheduleId;
                        lateCmd.Parameters.Add("@Lid", SqlDbType.Int).Value = loanId;
                        await lateCmd.ExecuteNonQueryAsync(stoppingToken);
                        _logger.LogWarning("Late marked: Loan {LoanId}, Schedule {ScheduleId}", loanId, scheduleId);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Monthly loan processor error");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
    