using System.Data;
using BankApp.BankApp.Common.Dtos.Transactions;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using Microsoft.Data.SqlClient;

namespace BankApp.BankApp.DataAccess;

public class TransactionDataAccess : ITransactionDataAccess
{
    private readonly DatabaseContext _context;

    public TransactionDataAccess(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<TransactionListDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Transactions_List");

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var transactions = new List<TransactionListDto>();
        while (await reader.ReadAsync(cancellationToken))
        {
            transactions.Add(MapTransactionList(reader));
        }

        return transactions;
    }

    public async Task<TransactionSelectDto?> SelectAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Transactions_Select");

        AddTransactionSelectParameters(command, transactionId);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapTransactionSelect(reader) : null;
    }

    public async Task<int> InsertAsync(
        TransactionCreateDto transaction,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Transactions_Insert");

        AddTransactionCreateParameters(command, transaction);

        await connection.OpenAsync(cancellationToken);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> UpdateAsync(TransactionUpdateDto transaction, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Transactions_Update");

        AddTransactionUpdateParameters(command, transaction);

        await connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(int transactionId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        using var command = CreateStoredProcedureCommand(connection, "sp_Transactions_Delete");

        AddTransactionDeleteParameters(command, transactionId);

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

    private static void AddTransactionCreateParameters(
        SqlCommand command,
        TransactionCreateDto transaction)
    {
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = transaction.AccountId;
        command.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 255).Value = transaction.TransactionType;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = transaction.Amount;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = transaction.CurrencyCode;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)transaction.Description ?? DBNull.Value;
    }

    private static void AddTransactionUpdateParameters(SqlCommand command, TransactionUpdateDto transaction)
    {
        command.Parameters.Add("@TransactionId", SqlDbType.Int).Value = transaction.TransactionId;
        command.Parameters.Add("@AccountId", SqlDbType.Int).Value = transaction.AccountId;
        command.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 255).Value = transaction.TransactionType;
        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = transaction.Amount;
        command.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3).Value = transaction.CurrencyCode;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = (object?)transaction.Description ?? DBNull.Value;
    }

    private static void AddTransactionDeleteParameters(SqlCommand command, int transactionId)
    {
        command.Parameters.Add("@TransactionId", SqlDbType.Int).Value = transactionId;
    }

    private static void AddTransactionSelectParameters(SqlCommand command, int transactionId)
    {
        command.Parameters.Add("@TransactionId", SqlDbType.Int).Value = transactionId;
    }

    private static TransactionListDto MapTransactionList(SqlDataReader reader)
    {
        return new TransactionListDto
        {
            TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            TransactionDate = GetOptionalDateTime(reader, "TransactionDate")
        };
    }

    private static TransactionSelectDto MapTransactionSelect(SqlDataReader reader)
    {
        return new TransactionSelectDto
        {
            TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
            AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
            TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
            TransactionDate = GetOptionalDateTime(reader, "TransactionDate"),
            Description = GetNullableString(reader, "Description")
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
