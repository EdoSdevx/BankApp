using BankApp.BankApp.Common.Enums;

namespace BankApp.BankApp.Common.Dtos.Employees;

public class EmployeeListDto
{
    public int EmployeeId { get; set; }
    public int BranchId { get; set; }
    public int RoleId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public AppRole AuthRole { get; set; }
}
