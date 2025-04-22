using QuanLyTruyenThong_TuVan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Repositories.Interfaces
{
    public interface IApartmentRepository
    {
        Task<Apartment> GetByIdAsync(int id);
        Task<IEnumerable<Apartment>> GetAllAsync();
        Task AddAsync(Apartment apartment);
        Task UpdateAsync(Apartment apartment);
        Task DeleteAsync(int id);
    }
}
