namespace BankApp.BankApp.Common.Dtos.Roles;

public class RoleSelectDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
