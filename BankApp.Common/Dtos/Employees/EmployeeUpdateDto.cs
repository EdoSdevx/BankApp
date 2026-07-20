using BankApp.BankApp.Common.Enums;

namespace BankApp.BankApp.Common.Dtos.Employees;

public class EmployeeUpdateDto
{
    public int EmployeeId { get; set; }
    public int? BranchId { get; set; }
    public int? RoleId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public AppRole? AuthRole { get; set; }
}
