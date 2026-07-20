namespace BankAppWPF.Models
{
    public class BranchListItem
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public string DisplayName =>
            $"{BranchName} ({BranchCode}) - {City}";
    }
}
