namespace BankApp.BankApp.Common.Dtos.Customer;

public class PendingTransferDto
{
    public int PendingTransferId { get; set; }
    public int SourceAccountId { get; set; }
    public int TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CreatedByCustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SrcFirstName { get; set; }
    public string? SrcLastName { get; set; }
    public string? TgtFirstName { get; set; }
    public string? TgtLastName { get; set; }
}
