namespace BankAppWPF.Models
{
    public class AccountCreateRequest
    {
        public int CustomerId { get; set; }
        public int BranchId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
