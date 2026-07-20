using BankApp.BankApp.Common;
using BankApp.BankApp.Common.Dtos.Auth;
using BankApp.BankApp.Common.Helpers;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Options;
using Microsoft.Extensions.Options;

namespace BankApp.BankApp.Services;

public class AuthService : IAuthService
{
    private readonly IAuthDataAccess _dataAccess;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ResetTokenOptions _resetTokenOptions;

    public AuthService(
        IAuthDataAccess dataAccess,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IOptions<ResetTokenOptions> resetTokenOptions)
    {
        _dataAccess = dataAccess;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _resetTokenOptions = resetTokenOptions.Value;
    }

    public async Task<Result<AuthLoginResultDto>> LoginAsync(AuthLoginDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateLogin(dto);
            if (validationResult is not null)
            {
                return validationResult;
            }

            dto.Email = dto.Email.Trim();

            var user = await _dataAccess.SelectEmployeeByEmailAsync(dto.Email, cancellationToken)
                ?? await _dataAccess.SelectCustomerByEmailAsync(dto.Email, cancellationToken);

            if (user is null)
            {
                return Result<AuthLoginResultDto>.Unauthorized("Email or password is wrong.");
            }

            var passwordIsValid = PasswordMatches(dto.Password, user.PasswordHash);
            if (!passwordIsValid)
            {
                return Result<AuthLoginResultDto>.Unauthorized("Email or password is wrong.");
            }

            var tokenResult = _jwtTokenService.CreateToken(user);
            var result = new AuthLoginResultDto
            {
                UserId = user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Role = user.Role,
                Token = tokenResult.Token,
                ExpiresAtUtc = tokenResult.ExpiresAtUtc
            };

            return Result<AuthLoginResultDto>.Ok(result, "Login successful.");
        }
        catch (Exception ex)
        {
            return Result<AuthLoginResultDto>.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return Result.Ok("If the email exists, a reset link has been sent.");
            }

            email = email.Trim();

            var employee = await _dataAccess.SelectEmployeeByEmailAsync(email, cancellationToken);
            if (employee is not null)
            {
                var expiresAt = DateTime.UtcNow.AddMinutes(_resetTokenOptions.ExpiresMinutes);
                var token = ResetTokenHelper.GenerateToken("Employee", employee.UserId, expiresAt, _resetTokenOptions.SecretKey);
                var resetLink = $"http://localhost:5173/reset-password?token={token}";
                await _emailService.SendPasswordResetEmailAsync(employee.Email, resetLink, cancellationToken);
                return Result.Ok("If the email exists, a reset link has been sent.");
            }

            var customer = await _dataAccess.SelectCustomerByEmailAsync(email, cancellationToken);
            if (customer is not null)
            {
                var expiresAt = DateTime.UtcNow.AddMinutes(_resetTokenOptions.ExpiresMinutes);
                var token = ResetTokenHelper.GenerateToken("Customer", customer.UserId, expiresAt, _resetTokenOptions.SecretKey);
                var resetLink = $"http://localhost:5173/reset-password?token={token}";
                await _emailService.SendPasswordResetEmailAsync(customer.Email, resetLink, cancellationToken);
            }

            return Result.Ok("If the email exists, a reset link has been sent.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    public async Task<Result> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return Result.Fail("New password is required.");
            }

            if (newPassword.Length < 6)
            {
                return Result.Fail("Password must be at least 6 characters.");
            }

            var tokenResult = ResetTokenHelper.ValidateToken(token, _resetTokenOptions.SecretKey);
            if (tokenResult is null)
            {
                return Result.Fail("Invalid or expired reset token.");
            }

            var (entityType, entityId) = tokenResult.Value;
            var passwordHash = PasswordHasher.Hash(newPassword);

            if (entityType == "Employee")
            {
                await _dataAccess.UpdateEmployeePasswordHashAsync(entityId, passwordHash, cancellationToken);
            }
            else if (entityType == "Customer")
            {
                await _dataAccess.UpdateCustomerPasswordHashAsync(entityId, passwordHash, cancellationToken);
            }
            else
            {
                return Result.Fail("Invalid reset token.");
            }

            return Result.Ok("Password has been reset successfully.");
        }
        catch (Exception ex)
        {
            return Result.DatabaseError(ex.Message);
        }
    }

    private static bool PasswordMatches(string password, string passwordHash)
    {
        try
        {
            return PasswordHasher.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    private static Result<AuthLoginResultDto>? ValidateLogin(AuthLoginDto dto)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.Email))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email is required."));
        else if (!dto.Email.Contains('@'))
            failures.Add(new ValidationFailure(nameof(dto.Email), "Email must contain '@'."));

        if (string.IsNullOrWhiteSpace(dto.Password))
            failures.Add(new ValidationFailure(nameof(dto.Password), "Password is required."));

        if (failures.Count > 0)
        {
            return new Result<AuthLoginResultDto>
            {
                Success = false,
                Message = "Validation failed",
                StatusCode = 400,
                ErrorCode = ResultErrorCode.ValidationError,
                Errors = failures
            };
        }

        return null;
    }
}
