namespace BankApp.BankApp.Common.Dtos.Accounts;

public class AccountUpdateDto
{
    public int AccountId { get; set; }
    public int? CustomerId { get; set; }
    public int? BranchId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? Balance { get; set; }
}
