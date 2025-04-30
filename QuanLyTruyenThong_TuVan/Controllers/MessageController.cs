using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;

        public MessageController(ApplicationDbContext context, UserManager<ApplicationResident> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Message
        public async Task<IActionResult> Index()
        {
            // Lấy user hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();    // hoặc RedirectToAction("Login", "Account");

            // Trích riêng user.Id ra biến đơn giản
            string userId = user.Id;

            // Dùng userId trong biểu thức LINQ
            var messages = await _context.InboxMessages
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }
        // GET: /Message/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var msg = await _context.InboxMessages
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);
            if (msg == null) return NotFound();

            if (!msg.IsRead)
            {
                msg.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return View(msg);
        }
    }
}
