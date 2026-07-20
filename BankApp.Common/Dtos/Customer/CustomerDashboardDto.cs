namespace BankApp.BankApp.Common.Dtos.Customer;

public class CustomerDashboardDto
{
    public int AccountCount { get; set; }
    public decimal TotalBalance { get; set; }
    public int UnpaidBillCount { get; set; }
}
