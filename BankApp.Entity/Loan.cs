namespace BankApp.BankApp.Entity;

public class Loan
{
    public int LoanId { get; set; }
    public int CustomerId { get; set; }
    public int LoanTypeId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public int DisbursementAccountId { get; set; }
    public int PaymentAccountId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int PaymentsMade { get; set; }
    public int PaymentsMissed { get; set; }
    public decimal RemainingPrincipal { get; set; }
}
