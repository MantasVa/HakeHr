using System.Data.SqlClient;

namespace HakeHR.Persistence.Data
{
    public interface IDbContext
    {
        SqlConnection GetConnection();
    }
}
