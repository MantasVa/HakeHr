using System.Collections.Generic;
using System.Threading.Tasks;
using HakeHR.Persistence.Models;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task AddEmployeeAsync(TeamEmployee teamEmployee);
        Task<IList<Team>> GetOrganizationTeamsAsync(int organizationId);
        Task<bool> AddPhotoPathAsync(int teamId, string photoPath);
        Task BulkInsertAsync(ICollection<Team> teams);
        Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> teamIds);
    }
}
