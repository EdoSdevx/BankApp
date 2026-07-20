namespace BankApp.BankApp.Common.Dtos.Customer;

public class AccountTransferDto
{
    public int SourceAccountId { get; set; }
    public int TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
