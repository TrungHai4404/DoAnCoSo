using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ForumController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Helper: xây dropdown từ enum
        private SelectList GetTypeTopicList(TypeTopic? sel = null)
        {
            var items = Enum.GetValues(typeof(TypeTopic))
                            .Cast<TypeTopic>()
                            .Select(t => new SelectListItem
                            {
                                Value = ((int)t).ToString(),
                                Text = t.ToString().Replace('_', ' '),
                                Selected = sel.HasValue && sel.Value == t
                            });
            return new SelectList(items, "Value", "Text",
                                  sel.HasValue ? (int)sel.Value : (int?)null);
        }

        // GET: /Forum
        // Cho phép tất cả xem, chỉ hiển thị bài đã duyệt
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var posts = await _db.ForumPosts
                                 .Include(fp => fp.Resident)
                                 .Where(fp => fp.IsApproved)
                                 .OrderByDescending(fp => fp.CreatedAt)
                                 .ToListAsync();
            return View(posts);
        }

        // GET: /Forum/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _db.ForumPosts
                                .Include(fp => fp.Resident)
                                .FirstOrDefaultAsync(fp => fp.Id == id && fp.IsApproved);
            if (post == null) return NotFound();
            return View(post);
        }

        // GET: /Forum/Create
        public IActionResult Create()
        {
            ViewData["TypeTopicList"] = GetTypeTopicList();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ViewData["YourName"] = _db.ApplicationResident.Find(userId)?.FullName;
            return View(new ForumPost { ResidentId = userId });
        }

        // POST: /Forum/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost model, IFormFile? imageFile)
        {
            // loại bỏ validation navigation
            ModelState.Remove(nameof(model.Forum));
            ModelState.Remove(nameof(model.Resident));

            // ép user và defaults
            model.ResidentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            model.CreatedAt = DateTime.Now;
            model.IsApproved = false;

            // xử lý ảnh
            if (imageFile?.Length > 0)
                model.ImageUrl = await SaveImageAsync(imageFile);

            if (!ModelState.IsValid)
            {
                ViewData["TypeTopicList"] = GetTypeTopicList(model.TypeTopic);
                ViewData["YourName"] = _db.ApplicationResident.Find(model.ResidentId)?.FullName;
                return View(model);
            }

            // tạo Forum để lấy TopicId
            var forum = new Forum { TypeTopic = model.TypeTopic };
            _db.Forums.Add(forum);
            await _db.SaveChangesAsync();

            model.TopicId = forum.Id;
            _db.ForumPosts.Add(model);
            await _db.SaveChangesAsync();

            // cập nhật PostId (nếu muốn)
            forum.PostId = model.Id;
            _db.Forums.Update(forum);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Bài của bạn đã gửi, vui lòng chờ Admin duyệt.";
            return RedirectToAction(nameof(Index));
        }

        // Helper: lưu ảnh vào wwwroot/uploads
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var fn = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(uploads, fn);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fn}";
        }
    }
}
