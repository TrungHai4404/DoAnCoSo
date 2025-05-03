// File: Controllers/CommentController.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            ApplicationDbContext context,
            UserManager<ApplicationResident> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<CommentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        // GET: /Comment/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            // Lấy các góp ý của riêng user đang đăng nhập
            var myComments = await _context.Comments
                .Where(c => c.ResidentId == user.Id)
                .Include(c => c.Responses)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(myComments);
        }


        // GET: /Comment/Inbox
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var myComments = await _context.Comments
                .Where(c => c.ResidentId == user.Id)
                .Include(c => c.Responses)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Trả về view với đường dẫn tuyệt đối đến Views/HopThu/Inbox.cshtml
            return View("~/Views/HopThu/Inbox.cshtml", myComments);
        }

        // GET: /Comment/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            ViewBag.ResidentName = user.FullName ?? user.UserName;
            ViewBag.ResidentId = user.Id;
            ViewBag.CommentTypes = Enum.GetValues(typeof(CommentType))
                .Cast<CommentType>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString().Replace("_", " ")
                })
                .ToList();

            return View(); // Views/Comment/Create.cshtml
        }

        // POST: /Comment/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Type")] Comment comment, IFormFile Image)
        {
            _logger.LogInformation("Starting Comment Create POST");

            // Loại bỏ các field không bind
            ModelState.Remove("Resident");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ResidentId");
            ModelState.Remove("ResidentName");
            ModelState.Remove("Status");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi góp ý.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Gán các trường bắt buộc
            comment.ResidentId = user.Id;
            comment.ResidentName = user.FullName ?? user.UserName;
            comment.Status = CommentStatus.Đã_Gửi;
            comment.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {
                // Chuẩn bị lại ViewBag và trả form nếu lỗi
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);

                ViewBag.ResidentName = comment.ResidentName;
                ViewBag.ResidentId = comment.ResidentId;
                ViewBag.CommentTypes = Enum.GetValues(typeof(CommentType))
                    .Cast<CommentType>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString().Replace("_", " ")
                    })
                    .ToList();

                return View(comment);
            }

            // Xử lý upload ảnh
            if (Image != null && Image.Length > 0)
            {
                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Image.FileName)}";
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Image.CopyToAsync(stream);

                comment.ImageUrl = "/images/" + fileName;
            }

            // Lưu góp ý
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Góp ý đã được gửi thành công!";

            // Redirect về Inbox
            return RedirectToAction(nameof(Inbox));
        }

        // GET: /Comment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                    .ThenInclude(r => r.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (comment.ResidentId != user.Id)
                return Forbid();

            // --- Đánh dấu tất cả response chưa đọc là đã đọc ---
            var unread = comment.Responses
                .Where(r => !r.IsRead)
                .ToList();
            if (unread.Any())
            {
                foreach (var r in unread)
                    r.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(comment);
        }

        // GET: /Comment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.Comments
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (comment.ResidentId != user.Id)
                return Forbid();

            return View(comment); // Views/Comment/Delete.cshtml
        }

        // POST: /Comment/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return RedirectToAction(nameof(Inbox));

            var user = await _userManager.GetUserAsync(User);
            if (comment.ResidentId != user.Id)
                return Forbid();

            if (!string.IsNullOrEmpty(comment.ImageUrl))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, comment.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa góp ý thành công!";
            return RedirectToAction(nameof(Inbox));
        }
    }
}
