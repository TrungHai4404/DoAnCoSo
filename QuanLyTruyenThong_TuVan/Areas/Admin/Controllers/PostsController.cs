// Areas/Admin/Controllers/PostController.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Post
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Sender)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        // GET: Admin/Post/Create
        public IActionResult Create()
        {
            PopulateSendersDropDown();
            ViewBag.Topics = new SelectList(Enum.GetValues(typeof(PostTopic)));
            return View();
        }

        // POST: Admin/Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,SenderId,Topic")] Post post, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    post.ImageUrl = await SaveImageAsync(imageFile);
                }
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateSendersDropDown(post.SenderId);
            ViewBag.Topics = new SelectList(Enum.GetValues(typeof(PostTopic)), post.Topic);
            return View(post);
        }

        // GET: Admin/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            PopulateSendersDropDown(post.SenderId);
            ViewBag.Topics = new SelectList(Enum.GetValues(typeof(PostTopic)), post.Topic);
            return View(post);
        }

        // POST: Admin/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageUrl,CreatedAt,SenderId,Topic")] Post post, IFormFile imageFile)
        {
            if (id != post.Id) return NotFound();
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var existing = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (!string.IsNullOrEmpty(existing.ImageUrl))
                            DeleteImage(existing.ImageUrl);
                        post.ImageUrl = await SaveImageAsync(imageFile);
                    }
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Posts.Any(e => e.Id == post.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateSendersDropDown(post.SenderId);
            ViewBag.Topics = new SelectList(Enum.GetValues(typeof(PostTopic)), post.Topic);
            return View(post);
        }

        // GET: Admin/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null) return NotFound();

            return View(post);
        }

        // POST: Admin/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                if (!string.IsNullOrEmpty(post.ImageUrl))
                    DeleteImage(post.ImageUrl);
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private void PopulateSendersDropDown(string selectedId = null)
        {
            ViewBag.Senders = new SelectList(
                _context.Users.Select(u => new { u.Id, u.FullName }),
                "Id", "FullName", selectedId);
        }
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);
            return $"/images/{fileName}";
        }
        private void DeleteImage(string imageUrl)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
        public async Task<IActionResult> Display(int? id)
        {
            if (id == null)
                return NotFound();

            var post = await _context.Posts
                                     .Include(p => p.Sender)
                                     .FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
                return NotFound();

            return View(post);
        }
    }
}
