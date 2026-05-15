using Microsoft.Data.SqlClient;

namespace ShopEZ.ProductService.Data
{
    public interface ISqlConnectionFactory
    {
        /// <summary>
        /// Returns a new (not yet opened) SqlConnection.
        /// Dapper opens the connection internally on first use.
        /// Caller is responsible for disposal — always use in a using block.
        /// </summary>
        SqlConnection CreateConnection();
    }

    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ProductDb")
                ?? throw new InvalidOperationException(
                       "Connection string 'ProductDb' is not configured.");
        }

        public SqlConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
