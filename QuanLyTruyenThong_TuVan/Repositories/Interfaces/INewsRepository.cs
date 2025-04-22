using QuanLyTruyenThong_TuVan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Repositories.Interfaces
{
    public interface INewsRepository
    {
        Task<New> GetByIdAsync(int id);
        Task<IEnumerable<New>> GetAllAsync();
        Task AddAsync(New news);
        Task UpdateAsync(New news);
        Task DeleteAsync(int id);
    }
}
