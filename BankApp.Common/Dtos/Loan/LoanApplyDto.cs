namespace BankApp.BankApp.Common.Dtos.Loan;

public class LoanApplyDto
{
    public int LoanTypeId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public int DisbursementAccountId { get; set; }
    public int PaymentAccountId { get; set; }
}
