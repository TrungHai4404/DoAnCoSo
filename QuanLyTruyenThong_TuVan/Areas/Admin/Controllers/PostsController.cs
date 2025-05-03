using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ForumPostController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ForumPostController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // helper build dropdown from enum
        private SelectList GetTypeTopicList(TypeTopic? selected = null)
        {
            var items = Enum.GetValues(typeof(TypeTopic))
                            .Cast<TypeTopic>()
                            .Select(t => new SelectListItem
                            {
                                Value = ((int)t).ToString(),
                                Text = t.ToString().Replace('_', ' '),
                                Selected = selected.HasValue && selected.Value == t
                            });
            return new SelectList(items, "Value", "Text",
                                  selected.HasValue ? (int)selected.Value : (int?)null);
        }

        // GET: Admin/ForumPost
        public async Task<IActionResult> Index()
        {
            var posts = await _db.ForumPosts
                                 .Include(fp => fp.Resident)
                                 .OrderByDescending(fp => fp.CreatedAt)
                                 .ToListAsync();
            return View(posts);
        }

        // GET: Admin/ForumPost/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var post = await _db.ForumPosts
                                .Include(fp => fp.Resident)
                                .FirstOrDefaultAsync(fp => fp.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        // GET: Admin/ForumPost/Create
        public IActionResult Create()
        {
            ViewData["TypeTopicList"] = GetTypeTopicList();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var resident = _db.ApplicationResident.Find(userId);
            ViewData["ResidentFullName"] = resident?.FullName;
            return View(new ForumPost { ResidentId = userId });
        }

        // POST: Admin/ForumPost/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost model, IFormFile? imageFile)
        {
            // enforce current user
            model.ResidentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ModelState.Remove(nameof(model.Resident));
            ModelState.Remove(nameof(model.Forum));

            if (imageFile?.Length > 0)
                model.ImageUrl = await SaveImageAsync(imageFile);

            if (!ModelState.IsValid)
            {
                ViewData["TypeTopicList"] = GetTypeTopicList(model.TypeTopic);
                var res = _db.ApplicationResident.Find(model.ResidentId);
                ViewData["ResidentFullName"] = res?.FullName;
                return View(model);
            }

            // create Forum record for FK
            var forum = new Forum { TypeTopic = model.TypeTopic };
            _db.Forums.Add(forum);
            await _db.SaveChangesAsync();

            model.TopicId = forum.Id;
            model.CreatedAt = DateTime.Now;
            model.IsApproved = false;
            _db.ForumPosts.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ForumPost/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            ViewData["TypeTopicList"] = GetTypeTopicList(post.TypeTopic);
            var res = _db.ApplicationResident.Find(post.ResidentId);
            ViewData["ResidentFullName"] = res?.FullName;
            return View(post);
        }

        // POST: Admin/ForumPost/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ForumPost model, IFormFile? imageFile)
        {
            if (id != model.Id) return BadRequest();
            ModelState.Remove(nameof(model.Resident));
            ModelState.Remove(nameof(model.Forum));

            if (imageFile?.Length > 0)
                model.ImageUrl = await SaveImageAsync(imageFile);
            else
                model.ImageUrl = (await _db.ForumPosts.AsNoTracking()
                    .FirstAsync(fp => fp.Id == id)).ImageUrl;

            if (!ModelState.IsValid)
            {
                ViewData["TypeTopicList"] = GetTypeTopicList(model.TypeTopic);
                var res = _db.ApplicationResident.Find(model.ResidentId);
                ViewData["ResidentFullName"] = res?.FullName;
                return View(model);
            }

            _db.ForumPosts.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/ForumPost/ToggleApproval/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post != null)
            {
                post.IsApproved = !post.IsApproved;
                _db.ForumPosts.Update(post);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ForumPost/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.ForumPosts
                                .Include(fp => fp.Resident)
                                .FirstOrDefaultAsync(fp => fp.Id == id);
            if (post == null) return NotFound();
            return View(post);
        }

        // POST: Admin/ForumPost/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post != null)
            {
                _db.ForumPosts.Remove(post);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // helper: save uploaded image
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
