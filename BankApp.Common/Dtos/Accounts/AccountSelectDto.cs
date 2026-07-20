namespace BankApp.BankApp.Common.Dtos.Accounts;

public class AccountSelectDto
{
    public int AccountId { get; set; }
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}
