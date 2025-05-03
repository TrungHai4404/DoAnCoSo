using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using QuanLyTruyenThong_TuVan.Data;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.Resident)
                .ToListAsync();
            return View(comments);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Resident)
                .Include(c => c.Responses)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.ResidentName = user.FullName ?? user.UserName;
                ViewBag.ResidentId = user.Id;
                // Thêm danh sách CommentType vào ViewBag
                ViewBag.CommentTypes = Enum.GetValues(typeof(CommentType))
                    .Cast<CommentType>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString().Replace("_", " ")
                    }).ToList();
            }
            else
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Type")] Comment comment, IFormFile Image)
        {
            _logger.LogInformation("Starting Comment Create POST action");

            ModelState.Remove("Resident");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ResidentId");
            ModelState.Remove("ResidentName");
            ModelState.Remove("Status");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User not authenticated");
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để gửi góp ý.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            comment.ResidentId = user.Id;
            comment.ResidentName = user.FullName ?? user.UserName;
            comment.Status = CommentStatus.Đã_Gửi;
            comment.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState invalid. Errors: {Errors}", string.Join("; ", errors));
                ViewBag.ResidentName = user.FullName ?? user.UserName;
                ViewBag.ResidentId = user.Id;
                // Khôi phục danh sách CommentType khi ModelState không hợp lệ
                ViewBag.CommentTypes = Enum.GetValues(typeof(CommentType))
                    .Cast<CommentType>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString().Replace("_", " ")
                    }).ToList();
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại: " + string.Join("; ", errors);
                return View(comment);
            }

            try
            {
                if (Image != null && Image.Length > 0)
                {
                    _logger.LogInformation("Processing image upload");
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    comment.ImageUrl = "/images/" + uniqueFileName;
                }

                _logger.LogInformation("Adding comment to database");
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Comment created successfully");
                TempData["SuccessMessage"] = "Góp ý đã được gửi thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                TempData["ErrorMessage"] = $"Lỗi khi lưu góp ý: {ex.Message}";
                ViewBag.ResidentName = user.FullName ?? user.UserName;
                ViewBag.ResidentId = user.Id;
                return View(comment);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Resident)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(comment.ImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, comment.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _context.Comments.Remove(comment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa góp ý thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting comment");
                    TempData["ErrorMessage"] = $"Lỗi khi xóa góp ý: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Comment/Inbox
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

            // Trả về view nằm ở Views/HopThu/Inbox.cshtml
            return View("~/Views/HopThu/Inbox.cshtml", myComments);
        }
    }
}