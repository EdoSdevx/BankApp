namespace BankApp.BankApp.Common.Constants;

public static class RoleNames
{
    public const string Customer = "Customer";
    public const string Employee = "Employee";
    public const string Admin = "Admin";
    public const string AdminOrEmployee = Admin + "," + Employee;
}
