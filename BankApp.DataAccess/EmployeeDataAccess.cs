using System.Data;
using BankApp.BankApp.Common.Dtos.Employees;
using BankApp.BankApp.Common.Enums;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class EmployeeDataAccess : IEmployeeDataAccess
{
    private readonly DatabaseContext _context;

    public EmployeeDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Employees_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var employees = new List<EmployeeListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            employees.Add(MapEmployeeList(reader));
        }

        return employees;
    }

    public async Task<EmployeeSelectDto?> SelectAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Employees_Select");

        AddEmployeeSelectParameters(command, employeeId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapEmployeeSelect(reader) : null;
    }

    public async Task<int> InsertAsync(
        EmployeeCreateDto employee,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Employees_Insert");

        AddEmployeeCreateParameters(command, employee, passwordHash);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(
        EmployeeUpdateDto employee,
        string? passwordHash = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Employees_Update");

        AddEmployeeUpdateParameters(command, employee, passwordHash);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Employees_Delete");

        AddEmployeeDeleteParameters(command, employeeId);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SqlCommand CreateStoredProcedureCommand(SqlConnection connection, string procedureName)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static void AddEmployeeCreateParameters(
        SqlCommand command,
        EmployeeCreateDto employee,
        string passwordHash)
    {
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = employee.BranchId;
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = employee.RoleId;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = employee.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = employee.LastName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = employee.Email;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = employee.Phone;
        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;
        command.Parameters.Add("@AuthRole", SqlDbType.NVarChar, 50).Value = employee.AuthRole.ToString();
    }

    private static void AddEmployeeUpdateParameters(
        SqlCommand command,
        EmployeeUpdateDto employee,
        string? passwordHash)
    {
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employee.EmployeeId;
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = employee.BranchId;
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = employee.RoleId;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = employee.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = employee.LastName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = employee.Email;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = employee.Phone;
        command.Parameters.Add("@AuthRole", SqlDbType.NVarChar, 50).Value =
            employee.AuthRole?.ToString() ?? AppRole.Employee.ToString();
        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = (object?)passwordHash ?? DBNull.Value;
    }

    private static void AddEmployeeDeleteParameters(SqlCommand command, int employeeId)
    {
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
    }

    private static void AddEmployeeSelectParameters(SqlCommand command, int employeeId)
    {
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
    }

    private static EmployeeListDto MapEmployeeList(SqlDataReader reader)
    {
        var firstName = reader.GetString(reader.GetOrdinal("FirstName"));
        var lastName = reader.GetString(reader.GetOrdinal("LastName"));

        return new EmployeeListDto
        {
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            FullName = $"{firstName} {lastName}",
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            AuthRole = GetEmployeeAuthRole(reader)
        };
    }

    private static EmployeeSelectDto MapEmployeeSelect(SqlDataReader reader)
    {
        return new EmployeeSelectDto
        {
            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            AuthRole = GetEmployeeAuthRole(reader),
            HireDate = GetOptionalDateTime(reader, "HireDate"),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"))
        };
    }

    private static AppRole GetEmployeeAuthRole(SqlDataReader reader)
    {
        var ordinal = reader.GetOrdinal("AuthRole");
        var authRole = reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);

        if (Enum.TryParse<AppRole>(authRole, ignoreCase: true, out var role))
        {
            if (role is AppRole.Admin or AppRole.Employee)
            {
                return role;
            }
        }

        return AppRole.Employee;
    }

    private static DateTime GetOptionalDateTime(SqlDataReader reader, string columnName)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return reader.IsDBNull(i) ? default : reader.GetDateTime(i);
            }
        }

        return default;
    }
}
