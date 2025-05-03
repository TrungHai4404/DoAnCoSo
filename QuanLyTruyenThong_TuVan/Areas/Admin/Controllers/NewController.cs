using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers {
    [Area("Admin")]
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
            // Giả sử bạn xác định "Administrator" bằng Email, UserName hoặc Role
            var adminUser = _context.Users
                .Where(u => u.FullName == "Administrator") // Hoặc Username == "admin"
                .Select(u => new { u.Id, u.FullName })
                .FirstOrDefault();

            ViewBag.Senders = new SelectList(new[] { adminUser }, "Id", "FullName", adminUser?.Id);



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
        public async Task<IActionResult> Create([Bind("Title,Notes,Content,ImagesUrl,SendAt,SenderId,DanhMuc,TrangThai")] New news, IFormFile imagesUrl) {
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
            var adminUser = _context.Users
                .Where(u => u.FullName == "Administrator") // Hoặc Username == "admin"
                .Select(u => new { u.Id, u.FullName })
                .FirstOrDefault();

            ViewBag.Senders = new SelectList(new[] { adminUser }, "Id", "FullName", adminUser?.Id);

            return View(news);
        }

        // POST: New/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Notes,Content,ImagesUrl,CreatedAt,SendAt,SenderId,DanhMuc,TrangThai")] New news, IFormFile imagesUrl) {
            if (id != news.Id) return NotFound();
            ModelState.Remove("ImagesUrl"); 

            if (ModelState.IsValid) {
                try {
                    var existingNews = await _context.News.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id);
                    if (existingNews == null) return NotFound();

                    // Nếu có hình mới
                    if (imagesUrl != null && imagesUrl.Length > 0) {
                        var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(imageDirectory)) {
                            Directory.CreateDirectory(imageDirectory);
                        }

                        // Nếu có hình cũ, xóa hình cũ
                        if (!string.IsNullOrEmpty(existingNews.ImagesUrl)) {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingNews.ImagesUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath)) {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu hình mới
                        var fileName = Path.GetFileName(imagesUrl.FileName);
                        var filePath = Path.Combine(imageDirectory, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create)) {
                            await imagesUrl.CopyToAsync(stream);
                        }

                        news.ImagesUrl = "/images/" + fileName;
                    }
                    else {
                        // Nếu không upload hình mới -> giữ lại hình cũ
                        news.ImagesUrl = existingNews.ImagesUrl;
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
    [Area("Admin")]
    public class CategoryController : Controller {
        // GET: Admin/Category
        public IActionResult Index() {
            var categories = Enum.GetValues(typeof(Category))
                                  .Cast<Category>()
                                  .Select(c => new {
                                      Id = (int)c,
                                      Name = c.ToString()
                                  }).ToList();

            return View(categories);
        }
    }
    [Area("Admin")]
    public class StatusController : Controller {
        // GET: Admin/Status
        public IActionResult Index() {
            var statuses = Enum.GetValues(typeof(Status))
                                .Cast<Status>()
                                .Select(s => new {
                                    Id = (int)s,
                                    Name = s.ToString()
                                }).ToList();

            return View(statuses);
        }
    }
}
