using BankApp.BankApp.Common.Dtos.Auth;

namespace BankApp.BankApp.Common.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthLoginResultDto>> LoginAsync(AuthLoginDto dto, CancellationToken cancellationToken = default);
    Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
