namespace BankApp.BankApp.Common.Dtos.Branches;

public class BranchCreateDto
{
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
