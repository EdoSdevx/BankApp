using BankApp.BankApp.Common.Dtos.Auth;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(AuthLoginUserDto user);
}
