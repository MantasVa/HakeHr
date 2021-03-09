
namespace HakeHR.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(string connectionString) :
            base(connectionString)
        {
        }

    }
}
