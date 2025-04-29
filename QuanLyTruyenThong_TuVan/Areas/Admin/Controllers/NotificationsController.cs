using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NotificationsController(ApplicationDbContext context,
                             UserManager<ApplicationResident> userManager,
                             IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .Include(n => n.Sender)
                .ToListAsync();
            return View(notifications);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            ViewBag.SenderName = notification.Sender?.FullName ?? notification.Sender?.UserName ?? "Không xác định";
            return View(notification);
        }
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.SenderName = user.FullName ?? user.UserName;
                ViewBag.SenderId = user.Id;
            }
            else
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification notification, IFormFile Image)
        {
            ModelState.Remove("Sender");
            ModelState.Remove("ImageUrl");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng hiện tại. Vui lòng đăng nhập lại.";
                Console.WriteLine("Lỗi: Không tìm thấy người dùng hiện tại.");
                ViewBag.SenderName = user?.FullName ?? user?.UserName;
                ViewBag.SenderId = user?.Id;
                return View(notification);
            }
            notification.SenderId = user.Id;

            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        var errorMessage = $"Lỗi ở trường {state.Key}: {error.ErrorMessage}";
                        Console.WriteLine(errorMessage);
                        errors.AppendLine(errorMessage);
                    }
                }
                TempData["ErrorMessage"] = errors.Length > 0 ? errors.ToString() : "Dữ liệu không hợp lệ.";
                ViewBag.SenderName = user.FullName ?? user.UserName;
                ViewBag.SenderId = user.Id;
                return View(notification);
            }
            try
            {
                if (Image != null && Image.Length > 0)
                {
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

                    notification.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi lưu: {ex.Message}";
                return View(notification);
            }
            ViewBag.SenderName = user.FullName ?? user.UserName;
            ViewBag.SenderId = user.Id;
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Sender) // Include Sender để lấy thông tin người gửi
                .FirstOrDefaultAsync(n => n.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            // Truyền tên người gửi vào ViewBag
            ViewBag.SenderName = notification.Sender?.FullName ?? notification.Sender?.UserName ?? "Không xác định";

            return View(notification);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Notification notification, IFormFile? Image)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Sender");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("SenderId");

            if (!ModelState.IsValid)
            {
                var errors = new StringBuilder();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        var errorMessage = $"Lỗi ở trường {state.Key}: {error.ErrorMessage}";
                        Console.WriteLine(errorMessage);
                        errors.AppendLine(errorMessage);
                    }
                }
                TempData["ErrorMessage"] = errors.Length > 0 ? errors.ToString() : "Dữ liệu không hợp lệ.";

                var notificationForError = await _context.Notifications
                    .Include(n => n.Sender)
                    .FirstOrDefaultAsync(n => n.Id == id);
                ViewBag.SenderName = notificationForError?.Sender?.FullName ?? notificationForError?.Sender?.UserName ?? "Không xác định";
                return View(notification);
            }

            var existingNotification = await _context.Notifications
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (existingNotification == null)
            {
                return NotFound();
            }

            if (Image != null && Image.Length > 0)
            {
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

                existingNotification.ImageUrl = "/images/" + uniqueFileName;
            }

            existingNotification.Title = notification.Title;
            existingNotification.Content = notification.Content;
            existingNotification.Type = notification.Type;
            existingNotification.TargetAudience = notification.TargetAudience;

            try
            {
                _context.Update(existingNotification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông báo thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật thông báo: {ex.Message}\nStackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Lỗi khi lưu: {ex.Message}";
                ViewBag.SenderName = existingNotification.Sender?.FullName ?? existingNotification.Sender?.UserName ?? "Không xác định";
                return View(notification);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Sender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(notification.ImageUrl))
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                            notification.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Xóa thông báo thành công!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi xóa thông báo: {ex.Message}";
                    Console.WriteLine($"Lỗi khi xóa thông báo: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}