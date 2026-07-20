namespace BankApp.BankApp.Common.Dtos.Customer;

public class ExchangeRequestDto
{
    public int SourceAccountId { get; set; }
    public int TargetAccountId { get; set; }
    public decimal TargetAmount { get; set; }
}
