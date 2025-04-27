using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Controllers {
    public class NewController : Controller {
        private readonly ApplicationDbContext _context;

        public NewController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: New
        public async Task<IActionResult> Index() {
            var news = await _context.News.Include(n => n.Sender).ToListAsync();
            return View(news);
        }

        // GET: New/Create
        public IActionResult Create() {
            // Load danh sách người gửi
            ViewBag.Senders = new SelectList(
                _context.Users.Select(u => new { u.Id, u.FullName }).ToList(),
                "Id", "FullName"
            );

            // Load danh sách hình ảnh có sẵn trong thư mục
            var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            var imageFiles = Directory.GetFiles(imageDirectory)
                                      .Select(file => Path.GetFileName(file))
                                      .ToList();
            ViewBag.Images = new SelectList(imageFiles);

            return View();
        }

        // POST: New/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Notes,Content,ImagesUrl,SendAt,SenderId,Category,Status")] New news, IFormFile imagesUrl) {
            if (ModelState.IsValid) {
                // Kiểm tra nếu có tệp hình ảnh được tải lên
                if (imagesUrl != null && imagesUrl.Length > 0) {
                    // Xác định đường dẫn thư mục lưu hình ảnh
                    var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(imageDirectory)) {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    // Lưu hình ảnh vào thư mục
                    var fileName = Path.GetFileName(imagesUrl.FileName);
                    var filePath = Path.Combine(imageDirectory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await imagesUrl.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn hình ảnh vào cơ sở dữ liệu
                    news.ImagesUrl = "/images/" + fileName;
                }

                news.CreatedAt = DateTime.Now; // Thêm thời gian tạo
                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, load lại danh sách Senders
            ViewBag.Senders = new SelectList(
                _context.Users.Select(u => new { u.Id, u.FullName }).ToList(),
                "Id", "FullName"
            );

            return View(news);
        }

        // GET: New/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            // Load danh sách người gửi (Senders) từ cơ sở dữ liệu
            ViewBag.Senders = new SelectList(
                _context.Users.Select(u => new { u.Id, u.FullName }).ToList(),
                "Id", "FullName", news.SenderId // Chọn người gửi hiện tại
            );

            return View(news);
        }

        // POST: New/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Notes,Content,ImagesUrl,CreatedAt,SendAt,SenderId,Category,Status")] New news, IFormFile imagesUrl) {
            if (id != news.Id) return NotFound();

            if (ModelState.IsValid) {
                try {
                    // Nếu có hình ảnh mới được tải lên
                    if (imagesUrl != null && imagesUrl.Length > 0) {
                        // Xác định đường dẫn thư mục lưu hình ảnh
                        var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(imageDirectory)) {
                            Directory.CreateDirectory(imageDirectory);
                        }

                        // Xóa hình ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(news.ImagesUrl)) {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", news.ImagesUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath)) {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu hình ảnh mới vào thư mục
                        var fileName = Path.GetFileName(imagesUrl.FileName);
                        var filePath = Path.Combine(imageDirectory, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create)) {
                            await imagesUrl.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn hình ảnh vào cơ sở dữ liệu
                        news.ImagesUrl = "/images/" + fileName;
                    }

                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!NewExists(news.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Senders = new SelectList(
                _context.Users.Select(u => new { u.Id, u.FullName }).ToList(),
                "Id", "FullName"
            );

            return View(news);
        }

        // GET: New/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) return NotFound();

            var news = await _context.News
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (news == null) return NotFound();

            return View(news);
        }

        // POST: New/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var news = await _context.News.FindAsync(id);
            if (news != null) {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool NewExists(int id) {
            return _context.News.Any(e => e.Id == id);
        }

        // GET: New/Display/5
        public async Task<IActionResult> Display(int? id) {
            if (id == null) {
                return NotFound();
            }

            var news = await _context.News
                .Include(n => n.Sender) // Load Sender thông qua quan hệ
                .FirstOrDefaultAsync(m => m.Id == id);

            if (news == null) {
                return NotFound();
            }

            return View(news);
        }
    }
}
