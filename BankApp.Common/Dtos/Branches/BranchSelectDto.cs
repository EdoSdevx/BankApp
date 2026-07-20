namespace BankApp.BankApp.Common.Dtos.Branches;

public class BranchSelectDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
