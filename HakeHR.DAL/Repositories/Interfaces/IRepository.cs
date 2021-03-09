using System.Collections.Generic;
using System.Threading.Tasks;

namespace HakeHR.Persistence.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<ICollection<T>> GetRecordsAsync(int? recordsPerPage, int pageNumber);
        Task<T> GetRecordByIdAsync(int id);
        Task InsertRecordAsync(T record);
        Task<bool> UpdateRecordAsync(T record);
        Task<bool> DeleteRecordAsync(int id);
    }
}
