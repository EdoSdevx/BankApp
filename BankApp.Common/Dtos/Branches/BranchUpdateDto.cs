namespace BankApp.BankApp.Common.Dtos.Branches;

public class BranchUpdateDto
{
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
}
