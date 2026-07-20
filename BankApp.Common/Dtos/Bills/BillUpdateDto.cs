namespace BankApp.BankApp.Common.Dtos.Bills;

public class BillUpdateDto
{
    public int BillId { get; set; }
    public int? CustomerId { get; set; }
    public string? BillType { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTime? DueDate { get; set; }
    public bool? IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
}
