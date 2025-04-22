using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using QuanLyTruyenThong_TuVan.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace QuanLyTruyenThong_TuVan.Repositories.EFRepository
{
    public class NewsRepository : INewsRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<New> GetByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task<IEnumerable<New>> GetAllAsync()
        {
            return await _context.News.ToListAsync();
        }

        public async Task AddAsync(New news)
        {
            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(New news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
        }
    }
}
