using Microsoft.Data.SqlClient;

namespace TcmbSimulator.Data;

public class TcmbDatabaseContext
{
    private readonly string _connectionString;

    public TcmbDatabaseContext(IConfiguration configuration)
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
