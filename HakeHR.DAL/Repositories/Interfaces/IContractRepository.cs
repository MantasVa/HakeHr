using System.Collections.Generic;
using System.Threading.Tasks;
using HakeHR.Persistence.Models;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface IContractRepository : IRepository<Contract>
    {
        Task<IList<Contract>> GetEmployeeContractsAsync(int employeeId);
        Task BulkInsertAsync(ICollection<Contract> contracts);
        Task BulkDeleteAsync(int? startIndex, int? endIndex, ICollection<int> contractIds);
    }
}
