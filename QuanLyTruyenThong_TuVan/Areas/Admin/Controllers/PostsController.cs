using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        // GET: Post
        public async Task<IActionResult> Index()
        {
            // Truy vấn danh sách bài viết và tên người gửi
            var posts = await _context.Posts
                .Select(p => new Post
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    SenderName = p.SenderName // Lấy thông tin SenderName
                })
                .ToListAsync();

            return View(posts); // Trả về view với danh sách bài viết
        }

        public IActionResult Create()
        {
            return View(); // Trả về view tạo bài viết mới
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,SenderName")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now; // Thêm thời gian tạo bài viết

                _context.Add(post); // Thêm bài viết vào DbContext
                await _context.SaveChangesAsync(); // Lưu vào cơ sở dữ liệu

                return RedirectToAction(nameof(Index)); // Quay lại danh sách bài viết
            }

            return View(post); // Nếu model không hợp lệ, trả về lại form tạo bài viết
        }
        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Nếu không tìm thấy id, trả về lỗi
            }

            var post = await _context.Posts.FindAsync(id); // Tìm bài viết theo id
            if (post == null)
            {
                return NotFound(); // Nếu bài viết không tồn tại, trả về lỗi
            }

            return View(post); // Trả về view để chỉnh sửa bài viết
        }

        // POST: Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,SenderName,CreatedAt")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound(); // Nếu id không khớp, trả về lỗi
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post); // Cập nhật bài viết trong DbContext
                    await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Posts.Any(e => e.Id == post.Id))
                    {
                        return NotFound(); // Nếu không tìm thấy bài viết, trả về lỗi
                    }
                    else
                    {
                        throw; // Ném lại lỗi nếu có vấn đề khác
                    }
                }
                return RedirectToAction(nameof(Index)); // Quay lại danh sách bài viết
            }

            return View(post); // Nếu model không hợp lệ, trả về form chỉnh sửa bài viết
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Nếu không tìm thấy id, trả về lỗi
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id); // Tìm bài viết theo id
            if (post == null)
            {
                return NotFound(); // Nếu bài viết không tồn tại, trả về lỗi
            }

            return View(post); // Trả về view xác nhận xóa bài viết
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id); // Tìm bài viết theo id
            if (post != null)
            {
                _context.Posts.Remove(post); // Xóa bài viết khỏi DbContext
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            }
            return RedirectToAction(nameof(Index)); // Quay lại danh sách bài viết
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Nếu không tìm thấy id, trả về lỗi
            }

            // Tìm bài viết theo id mà không dùng Include (vì SenderName không phải là navigation property)
            var post = await _context.Posts
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync(); // Trả về bài viết hoặc null nếu không tìm thấy

            if (post == null)
            {
                return NotFound(); // Nếu bài viết không tồn tại, trả về lỗi
            }

            return View(post); // Trả về view chi tiết bài viết
        }

    }
}
