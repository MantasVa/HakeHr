using System.Data.SqlClient;

namespace HakeHR.Persistence.Data
{
    public abstract class DbContext : IDbContext
    {
        private readonly string connectionString;

        protected DbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlConnection GetConnection() => new SqlConnection(connectionString);
    }
}
