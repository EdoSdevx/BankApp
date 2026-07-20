using BankApp.BankApp.Common.Dtos.Auth;

namespace BankApp.BankApp.Common.Interfaces.DataAccess;

public interface IAuthDataAccess
{
    Task<AuthLoginUserDto?> SelectEmployeeByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthLoginUserDto?> SelectCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> UpdateEmployeePasswordHashAsync(int employeeId, string passwordHash, CancellationToken cancellationToken = default);
    Task<int> UpdateCustomerPasswordHashAsync(int customerId, string passwordHash, CancellationToken cancellationToken = default);
}
