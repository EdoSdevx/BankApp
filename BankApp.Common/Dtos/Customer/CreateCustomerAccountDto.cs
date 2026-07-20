namespace BankApp.BankApp.Common.Dtos.Customer;

public class CreateCustomerAccountDto
{
    public int BranchId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
}
