using BankApp.BankApp.Common.Enums;

namespace BankApp.BankApp.Common.Dtos.Auth;

public class AuthLoginResultDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public AppRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
