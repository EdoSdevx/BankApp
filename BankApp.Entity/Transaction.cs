namespace BankApp.BankApp.Entity;

public class Transaction
{
    public int TransactionId { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
}
