namespace BankApp.BankApp.Entity;

public class LoanSchedule
{
    public int ScheduleId { get; set; }
    public int LoanId { get; set; }
    public int PeriodNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal TotalDue { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsLate { get; set; }
}
