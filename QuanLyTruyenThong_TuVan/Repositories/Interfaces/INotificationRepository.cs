using QuanLyTruyenThong_TuVan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(int id);
        Task<IEnumerable<Notification>> GetAllAsync();
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(int id);
    }
}
