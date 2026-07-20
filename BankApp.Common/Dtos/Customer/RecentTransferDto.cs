namespace BankApp.BankApp.Common.Dtos.Customer;

public class RecentTransferDto
{
    public int TransactionId { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public int? RelatedAccountId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RelatedCurrencyCode { get; set; }
}
