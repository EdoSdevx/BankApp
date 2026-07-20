using System.Data;
using BankApp.BankApp.Common.Dtos.Branches;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class BranchDataAccess : IBranchDataAccess
{
    private readonly DatabaseContext _context;

    public BranchDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<BranchListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_List");

        await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var branches = new List<BranchListDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            branches.Add(MapBranchList(reader));
        }

        return branches;
    }

    public async Task<BranchSelectDto?> SelectAsync(int branchId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_Select");

        AddBranchSelectParameters(command, branchId);

        await connection.OpenAsync(cancellationToken);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapBranchSelect(reader);
        }

        return null;
    }

    public async Task<int> InsertAsync(
        BranchCreateDto branch,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_Insert");

        AddBranchCreateParameters(command, branch);

        await connection.OpenAsync(cancellationToken);

        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(BranchUpdateDto branch, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_Update");

        AddBranchUpdateParameters(command, branch);

        await connection.OpenAsync(cancellationToken);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int branchId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Branches_Delete");

        AddBranchDeleteParameters(command, branchId);

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

    private static void AddBranchCreateParameters(SqlCommand command, BranchCreateDto branch)
    {
        command.Parameters.Add("@BranchName", SqlDbType.NVarChar, 255).Value = branch.BranchName;
        command.Parameters.Add("@BranchCode", SqlDbType.NVarChar, 255).Value = branch.BranchCode;
        command.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = branch.City;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = branch.Address;
    }

    private static void AddBranchUpdateParameters(SqlCommand command, BranchUpdateDto branch)
    {
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = branch.BranchId;
        command.Parameters.Add("@BranchName", SqlDbType.NVarChar, 255).Value = branch.BranchName;
        command.Parameters.Add("@BranchCode", SqlDbType.NVarChar, 255).Value = branch.BranchCode;
        command.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = branch.City;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = branch.Address;
    }

    private static void AddBranchDeleteParameters(SqlCommand command, int branchId)
    {
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
    }

    private static void AddBranchSelectParameters(SqlCommand command, int branchId)
    {
        command.Parameters.Add("@BranchId", SqlDbType.Int).Value = branchId;
    }

    private static BranchListDto MapBranchList(SqlDataReader reader)
    {
        return new BranchListDto
        {
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
            BranchCode = reader.GetString(reader.GetOrdinal("BranchCode")),
            City = reader.GetString(reader.GetOrdinal("City"))
        };
    }

    private static BranchSelectDto MapBranchSelect(SqlDataReader reader)
    {
        return new BranchSelectDto
        {
            BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
            BranchName = reader.GetString(reader.GetOrdinal("BranchName")),
            BranchCode = reader.GetString(reader.GetOrdinal("BranchCode")),
            City = reader.GetString(reader.GetOrdinal("City")),
            Address = reader.GetString(reader.GetOrdinal("Address")),
            CreatedDate = GetOptionalDateTime(reader, "CreatedDate")
        };
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
