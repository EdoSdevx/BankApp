namespace BankAppWPF.Models
{
    public class AccountUpdateRequest
    {
        public int AccountId { get; set; }
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? Balance { get; set; }
    }
}
