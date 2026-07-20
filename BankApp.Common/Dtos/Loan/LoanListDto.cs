namespace BankApp.BankApp.Common.Dtos.Loan;

public class LoanListDto
{
    public int LoanId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public string? LoanTypeName { get; set; }
    public int LoanTypeId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int PaymentsMade { get; set; }
    public int PaymentsMissed { get; set; }
    public decimal RemainingPrincipal { get; set; }
}
