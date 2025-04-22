using QuanLyTruyenThong_TuVan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment> GetByIdAsync(int id);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);
    }
}
