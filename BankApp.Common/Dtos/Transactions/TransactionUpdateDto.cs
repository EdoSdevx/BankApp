namespace BankApp.BankApp.Common.Dtos.Transactions;

public class TransactionUpdateDto
{
    public int TransactionId { get; set; }
    public int? AccountId { get; set; }
    public string? TransactionType { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Description { get; set; }
}
