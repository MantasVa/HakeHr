using System.Collections.Generic;
using System.Threading.Tasks;
using HakeHR.Persistence.Models;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task AddContractAsync(EmployeeContract employeeContract);
        Task<IList<Employee>> GetTeamMembersAsync(int teamId);
        Task<bool> AddPhotoPathAsync(int employeeId, string photoPath);
        Task BulkInsertAsync(ICollection<Employee> employees);
        Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> employeeIds);
    }
}
