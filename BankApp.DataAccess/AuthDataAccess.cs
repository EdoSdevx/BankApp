using System.Data;
using BankApp.BankApp.Common.Dtos.Auth;
using BankApp.BankApp.Common.Enums;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class AuthDataAccess : IAuthDataAccess
{
    private readonly DatabaseContext _context;

    public AuthDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<AuthLoginUserDto?> SelectEmployeeByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateEmployeeByEmailCommand(connection);

        AddSelectByEmailParameters(command, email);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapEmployeeLogin(reader);
        }

        return null;
    }

    public async Task<AuthLoginUserDto?> SelectCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateCustomerByEmailCommand(connection);

        AddSelectByEmailParameters(command, email);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapCustomerLogin(reader);
        }

        return null;
    }

    private static SqlCommand CreateEmployeeByEmailCommand(SqlConnection connection)
    {
        return new SqlCommand(
            """
            SELECT TOP 1
                EmployeeId,
                FirstName,
                LastName,
                Email,
                PasswordHash,
                AuthRole
            FROM Employees
            WHERE Email = @Email;
            """,
            connection)
        {
            CommandType = CommandType.Text
        };
    }

    private static SqlCommand CreateCustomerByEmailCommand(SqlConnection connection)
    {
        return new SqlCommand(
            """
            SELECT TOP 1
                CustomerId,
                FirstName,
                LastName,
                Email,
                PasswordHash
            FROM Customers
            WHERE Email = @Email AND IsActive = 1;
            """,
            connection)
        {
            CommandType = CommandType.Text
        };
    }

    private static void AddSelectByEmailParameters(SqlCommand command, string email)
    {
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;
    }

    private static AuthLoginUserDto MapEmployeeLogin(SqlDataReader reader)
    {
        var authRole = GetNullableString(reader, "AuthRole");
        var role = ParseEmployeeRole(authRole);

        return new AuthLoginUserDto
        {
            UserId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = role
        };
    }

    private static AuthLoginUserDto MapCustomerLogin(SqlDataReader reader)
    {
        return new AuthLoginUserDto
        {
            UserId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = AppRole.Customer
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static AppRole ParseEmployeeRole(string? authRole)
    {
        if (Enum.TryParse<AppRole>(authRole, ignoreCase: true, out var role))
        {
            if (role is AppRole.Admin or AppRole.Employee)
            {
                return role;
            }
        }

        return AppRole.Employee;
    }

    public async Task<int> UpdateEmployeePasswordHashAsync(int employeeId, string passwordHash, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand(
            "UPDATE Employees SET PasswordHash = @PasswordHash WHERE EmployeeId = @EmployeeId",
            connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> UpdateCustomerPasswordHashAsync(int customerId, string passwordHash, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = new SqlCommand(
            "UPDATE Customers SET PasswordHash = @PasswordHash WHERE CustomerId = @CustomerId",
            connection)
        {
            CommandType = CommandType.Text
        };

        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
