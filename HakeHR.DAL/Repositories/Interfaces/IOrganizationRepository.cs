using System.Collections.Generic;
using System.Threading.Tasks;
using HakeHR.Persistence.Models;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task AddTeamAsync(TeamOrganization teamOrganization);
        Task<bool> AddPhotoPathAsync(int organizationId, string photoPath);
        Task BulkInsertAsync(ICollection<Organization> organizations);
        Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> organizationIds);
    }
}
