using System.Data;
using BankApp.BankApp.Common.Dtos.Currencies;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class CurrencyDataAccess : ICurrencyDataAccess
{
    private readonly DatabaseContext _context;

    public CurrencyDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var currencies = new List<CurrencyListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            currencies.Add(MapCurrencyList(reader));
        }

        return currencies;
    }

    public async Task<CurrencySelectDto?> SelectAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_Select");

        AddCurrencySelectParameters(command, currencyCode);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapCurrencySelect(reader) : null;
    }

    public async Task<int> InsertAsync(CurrencyCreateDto currency, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_Insert");

        AddCurrencyCreateParameters(command, currency);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync(CurrencyUpdateDto currency, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_Update");

        AddCurrencyUpdateParameters(command, currency);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(string currencyCode, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Currencies_Delete");

        AddCurrencyDeleteParameters(command, currencyCode);

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

    private static void AddCurrencyCreateParameters(SqlCommand command, CurrencyCreateDto currency)
    {
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currency.CurrencyCode;
        command.Parameters.Add("@CurrencyName", SqlDbType.NVarChar, 255).Value = currency.CurrencyName;
    }

    private static void AddCurrencyUpdateParameters(SqlCommand command, CurrencyUpdateDto currency)
    {
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currency.CurrencyCode;
        command.Parameters.Add("@CurrencyName", SqlDbType.NVarChar, 255).Value = currency.CurrencyName;
    }

    private static void AddCurrencyDeleteParameters(SqlCommand command, string currencyCode)
    {
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currencyCode;
    }

    private static void AddCurrencySelectParameters(SqlCommand command, string currencyCode)
    {
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = currencyCode;
    }

    private static CurrencyListDto MapCurrencyList(SqlDataReader reader)
    {
        return new CurrencyListDto
        {
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            CurrencyName = reader.GetString(reader.GetOrdinal("CurrencyName"))
        };
    }

    private static CurrencySelectDto MapCurrencySelect(SqlDataReader reader)
    {
        return new CurrencySelectDto
        {
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            CurrencyName = reader.GetString(reader.GetOrdinal("CurrencyName"))
        };
    }
}
