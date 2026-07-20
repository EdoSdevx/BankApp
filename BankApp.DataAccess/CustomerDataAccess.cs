using System.Data;
using BankApp.BankApp.Common.Dtos.Customers;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class CustomerDataAccess : ICustomerDataAccess
{
    private readonly DatabaseContext _context;

    public CustomerDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customers_List");

        await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var customers = new List<CustomerListDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(MapCustomerList(reader));
        }

        return customers;
    }

    public async Task<CustomerSelectDto?> SelectAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customers_Select");

        AddCustomerSelectParameters(command, customerId);

        await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapCustomerSelect(reader);
        }

        return null;
    }

    public async Task<int> InsertAsync(
        CustomerCreateDto customer,
        string passwordHash,
        bool isActive = true,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customers_Insert");

        AddCustomerCreateParameters(command, customer, passwordHash, isActive);

        await connection.OpenAsync(cancellationToken);

        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(
        CustomerUpdateDto customer,
        string? passwordHash = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customers_Update");

        AddCustomerUpdateParameters(command, customer, passwordHash);

        await connection.OpenAsync(cancellationToken);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Customers_Delete");

        AddCustomerDeleteParameters(command, customerId);

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

    private static void AddCustomerCreateParameters(
        SqlCommand command,
        CustomerCreateDto customer,
        string passwordHash,
        bool isActive)
    {
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = customer.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = customer.LastName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = customer.Email;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = (object?)customer.Phone ?? DBNull.Value;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = customer.Address;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = passwordHash;
    }

    private static void AddCustomerUpdateParameters(
        SqlCommand command,
        CustomerUpdateDto customer,
        string? passwordHash)
    {
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customer.CustomerId;
        command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = customer.FirstName;
        command.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = customer.LastName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = customer.Email;
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 255).Value = (object?)customer.Phone ?? DBNull.Value;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = customer.Address;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = customer.IsActive;
        command.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 255).Value = (object?)passwordHash ?? DBNull.Value;
    }

    private static void AddCustomerDeleteParameters(SqlCommand command, int customerId)
    {
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
    }

    private static void AddCustomerSelectParameters(SqlCommand command, int customerId)
    {
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
    }

    private static CustomerListDto MapCustomerList(SqlDataReader reader)
    {
        var firstName = reader.GetString(reader.GetOrdinal("FirstName"));
        var lastName = reader.GetString(reader.GetOrdinal("LastName"));

        return new CustomerListDto
        {
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            FullName = $"{firstName} {lastName}",
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = GetNullableString(reader, "Phone"),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    private static CustomerSelectDto MapCustomerSelect(SqlDataReader reader)
    {
        return new CustomerSelectDto
        {
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = GetNullableString(reader, "Phone"),
            Address = reader.GetString(reader.GetOrdinal("Address")),
            CreatedDate = GetOptionalDateTime(reader, "CreatedDate"),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash"))
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
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
