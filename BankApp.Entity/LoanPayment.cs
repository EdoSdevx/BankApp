namespace BankApp.BankApp.Entity;

public class LoanPayment
{
    public int PaymentId { get; set; }
    public int? ScheduleId { get; set; }
    public int LoanId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
}
