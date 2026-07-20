namespace BankApp.BankApp.Common.Dtos.Loan;

public class LoanPaymentDto
{
    public int PaymentId { get; set; }
    public int? ScheduleId { get; set; }
    public int LoanId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
}
