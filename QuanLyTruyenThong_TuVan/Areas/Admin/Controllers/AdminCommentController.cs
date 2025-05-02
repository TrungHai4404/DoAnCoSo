// File: Areas/Admin/Controllers/AdminCommentController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class AdminCommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;
        private readonly IEmailSender _emailSender;

        public AdminCommentController(
            ApplicationDbContext context,
            UserManager<ApplicationResident> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Admin/AdminComment
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                .ToListAsync();
            return View(comments);
        }

        // GET: Admin/AdminComment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var comment = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                    .ThenInclude(r => r.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            return View(comment);
        }

        // POST: Admin/AdminComment/Respond
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int commentId, string content, CommentStatus status)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction(nameof(Details), new { id = commentId });
            }

            // Tìm comment gốc
            var comment = await _context.Comments
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy góp ý.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật trạng thái xử lý
            comment.Status = status;

            // Lấy admin hiện tại
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập với quyền Admin.";
                return RedirectToAction(nameof(Details), new { id = commentId });
            }

            // Tạo record Response và gán TitleComment để tránh NULL
            var response = new Response
            {
                CommentId = commentId,
                ResidentId = admin.Id,
                TitleComment = comment.Title,    // <-- gán tiêu đề góp ý gốc
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            // Gửi email thông báo cho user
            var user = comment.Resident;
            var subject = $"[Mochi] Phản hồi góp ý của bạn: \"{comment.Title}\"";
            var message = $@"
                Xin chào {user.FullName ?? user.UserName},<br/><br/>
                Góp ý của bạn đã được cập nhật trạng thái: <strong>{status}</strong>.<br/>
                <em>Nội dung phản hồi từ Admin:</em><br/>
                {content}<br/><br/>
                Cảm ơn bạn đã đóng góp ý kiến!<br/>
                --<br/>Mochi Team";

            await _emailSender.SendEmailAsync(user.Email, subject, message);

            TempData["SuccessMessage"] = "Phản hồi đã được gửi và email đã được thông báo đến người dùng.";
            return RedirectToAction(nameof(Details), new { id = commentId });
        }
    }
}
