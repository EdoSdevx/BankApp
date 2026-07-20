using BankApp.BankApp.Common.Enums;

namespace BankApp.BankApp.Common.Dtos.Employees;

public class EmployeeCreateDto
{
    public int BranchId { get; set; }
    public int RoleId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public AppRole AuthRole { get; set; } = AppRole.Employee;
}
