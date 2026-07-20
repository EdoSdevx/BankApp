namespace BankApp.BankApp.Common.Dtos.Transactions;

public class TransactionCreateDto
{
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Description { get; set; }
}
