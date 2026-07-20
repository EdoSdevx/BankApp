using System.Data;
using BankApp.BankApp.Common.Dtos.ExchangeRates;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class ExchangeRateDataAccess : IExchangeRateDataAccess
{
    private readonly DatabaseContext _context;

    public ExchangeRateDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<ExchangeRateListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rates = new List<ExchangeRateListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rates.Add(MapExchangeRateList(reader));
        }

        return rates;
    }

    public async Task<ExchangeRateSelectDto?> SelectAsync(int rateId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_Select");

        AddExchangeRateSelectParameters(command, rateId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapExchangeRateSelect(reader) : null;
    }

    public async Task<int> InsertAsync(ExchangeRateCreateDto rate, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_Insert");

        AddExchangeRateCreateParameters(command, rate);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(ExchangeRateUpdateDto rate, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_Update");

        AddExchangeRateUpdateParameters(command, rate);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int rateId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_ExchangeRates_Delete");

        AddExchangeRateDeleteParameters(command, rateId);

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

    private static void AddExchangeRateCreateParameters(SqlCommand command, ExchangeRateCreateDto rate)
    {
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = rate.CurrencyCode;
        command.Parameters.Add("@Rate", SqlDbType.Decimal).Value = rate.Rate;
        command.Parameters.Add("@Source", SqlDbType.NVarChar, 255).Value = rate.Source;
    }

    private static void AddExchangeRateUpdateParameters(SqlCommand command, ExchangeRateUpdateDto rate)
    {
        command.Parameters.Add("@RateId", SqlDbType.Int).Value = rate.RateId;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = rate.CurrencyCode;
        command.Parameters.Add("@Rate", SqlDbType.Decimal).Value = rate.Rate;
        command.Parameters.Add("@Source", SqlDbType.NVarChar, 255).Value = rate.Source;
    }

    private static void AddExchangeRateDeleteParameters(SqlCommand command, int rateId)
    {
        command.Parameters.Add("@RateId", SqlDbType.Int).Value = rateId;
    }

    private static void AddExchangeRateSelectParameters(SqlCommand command, int rateId)
    {
        command.Parameters.Add("@RateId", SqlDbType.Int).Value = rateId;
    }

    private static ExchangeRateListDto MapExchangeRateList(SqlDataReader reader)
    {
        return new ExchangeRateListDto
        {
            RateId = reader.GetInt32(reader.GetOrdinal("RateId")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Rate = reader.GetDecimal(reader.GetOrdinal("Rate")),
            RateDate = GetOptionalDateTime(reader, "RateDate"),
            Source = reader.GetString(reader.GetOrdinal("Source"))
        };
    }

    private static ExchangeRateSelectDto MapExchangeRateSelect(SqlDataReader reader)
    {
        return new ExchangeRateSelectDto
        {
            RateId = reader.GetInt32(reader.GetOrdinal("RateId")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            Rate = reader.GetDecimal(reader.GetOrdinal("Rate")),
            RateDate = GetOptionalDateTime(reader, "RateDate"),
            Source = reader.GetString(reader.GetOrdinal("Source"))
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
