using System.Data;
using BankApp.BankApp.Common.Dtos.Roles;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class RoleDataAccess : IRoleDataAccess
{
    private readonly DatabaseContext _context;

    public RoleDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<RoleListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Roles_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var roles = new List<RoleListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(MapRoleList(reader));
        }

        return roles;
    }

    public async Task<RoleSelectDto?> SelectAsync(int roleId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Roles_Select");

        AddRoleSelectParameters(command, roleId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapRoleSelect(reader) : null;
    }

    public async Task<int> InsertAsync(RoleCreateDto role, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Roles_Insert");

        AddRoleCreateParameters(command, role);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(RoleUpdateDto role, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Roles_Update");

        AddRoleUpdateParameters(command, role);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int roleId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Roles_Delete");

        AddRoleDeleteParameters(command, roleId);

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

    private static void AddRoleCreateParameters(SqlCommand command, RoleCreateDto role)
    {
        command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 255).Value = role.RoleName;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)role.Description ?? DBNull.Value;
    }

    private static void AddRoleUpdateParameters(SqlCommand command, RoleUpdateDto role)
    {
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = role.RoleId;
        command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 255).Value = role.RoleName;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)role.Description ?? DBNull.Value;
    }

    private static void AddRoleDeleteParameters(SqlCommand command, int roleId)
    {
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
    }

    private static void AddRoleSelectParameters(SqlCommand command, int roleId)
    {
        command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
    }

    private static RoleListDto MapRoleList(SqlDataReader reader)
    {
        return new RoleListDto
        {
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
            Description = GetNullableString(reader, "Description")
        };
    }

    private static RoleSelectDto MapRoleSelect(SqlDataReader reader)
    {
        return new RoleSelectDto
        {
            RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
            RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
            Description = GetNullableString(reader, "Description")
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
