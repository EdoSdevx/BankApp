using Microsoft.Data.SqlClient;

namespace BankApp2.Data;

public class RecipientDatabaseContext
{
    private readonly string _connectionString;

    public RecipientDatabaseContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection connection string is missing.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
