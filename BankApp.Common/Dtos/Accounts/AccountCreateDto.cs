namespace BankApp.BankApp.Common.Dtos.Accounts;

public class AccountCreateDto
{
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
