using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;

        public CommentController(
            ApplicationDbContext context,
            UserManager<ApplicationResident> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Comment
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(comments);
        }

        // GET: Admin/Comment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                    .ThenInclude(r => r.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();
            return View(comment);
        }

        // GET: Admin/Comment/Respond/5
        public async Task<IActionResult> Respond(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            ViewBag.Comment = comment;
            return View();
        }

        // POST: Admin/Comment/Respond/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int id, string responseContent)
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(responseContent))
                ModelState.AddModelError("responseContent", "Nội dung phản hồi không được trống.");

            // 2. Load comment
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.ResponseContent = responseContent;
                return View();
            }

            // 3. Lấy admin hiện tại
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null) return Challenge();

            // 4. Tạo Response
            var reply = new Response
            {
                CommentId = comment.Id,
                TitleComment = $"Phản hồi: {comment.Title}",
                Content = responseContent,
                CreatedAt = DateTime.Now,
                ResidentId = admin.Id
            };

            // 5. Tạo InboxMessage cho user
            var inbox = new InboxMessage
            {
                UserId = comment.ResidentId,
                Title = $"Phản hồi góp ý #{comment.Id}",
                Content = responseContent,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            // 6. Lưu cả hai vào DB
            _context.Responses.Add(reply);
            _context.InboxMessages.Add(inbox);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã gửi phản hồi và thông báo đến người dùng";
            return RedirectToAction(nameof(Details), new { id = comment.Id });
        }

        // GET: Admin/Comment/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();
            return View(comment);
        }

        // POST: Admin/Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                // Xóa file ảnh nếu có
                if (!string.IsNullOrEmpty(comment.ImageUrl))
                {
                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        comment.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa góp ý thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
