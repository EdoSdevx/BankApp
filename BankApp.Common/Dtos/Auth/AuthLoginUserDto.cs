using BankApp.BankApp.Common.Enums;

namespace BankApp.BankApp.Common.Dtos.Auth;

public class AuthLoginUserDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AppRole Role { get; set; }
}
