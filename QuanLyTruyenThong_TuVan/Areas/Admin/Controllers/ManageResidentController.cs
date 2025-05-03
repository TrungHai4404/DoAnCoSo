using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models;
using QuanLyTruyenThong_TuVan.Repositories.Interfaces;
namespace QuanLyTruyenThong_TuVan.Controllers
{
    //[Authorize(Roles = "Admin")] // Chỉ admin truy cập được
    [Area("Admin")]
    public class ManageResidentController : Controller
    {
        private readonly UserManager<ApplicationResident> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IApartmentRepository _apartmentRepository;

        public ManageResidentController(UserManager<ApplicationResident> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, IApartmentRepository apartmentRepository)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _apartmentRepository = apartmentRepository;
        }
        public async Task<IActionResult> Index()
        {
            var residents = await _userManager.Users.ToListAsync();
            var apartment = await _apartmentRepository.GetAllAsync();
            ViewBag.Apartments = apartment;
            return View(residents);
        }

        //Tạo tài khoản
        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewBag.AllRoles = _roleManager.Roles.ToList();
            ViewBag.Apartments = _apartmentRepository.GetAllAsync().Result; // Lấy danh sách căn hộ từ repository
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string fullName,string gender, int? ApartmentId, string[] selectedRoles) // Thêm selectedRoles
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationResident { UserName = email, Email = email, FullName = fullName,Gender = gender, ApartmentId= ApartmentId };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        foreach (var role in selectedRoles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            ViewBag.AllRoles = _roleManager.Roles.ToList();
            return View();
        }

        // Chỉnh sửa tài khoản
        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.AllRoles = _roleManager.Roles.ToList();
            ViewBag.Apartments = await _apartmentRepository.GetAllAsync(); // Lấy danh sách căn hộ từ repository
            return View(user);
        }
        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string password, int ApartmentId, string[] selectedRoles)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id) as ApplicationResident;

            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật mật khẩu nếu có truyền vào
            if (!string.IsNullOrWhiteSpace(password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(user); // hoặc return View(model) nếu bạn có view model
                }
            }

            // Cập nhật ApartmentId
            if(ApartmentId == 0)
            {
                user.ApartmentId = null; // Nếu ApartmentId là 0, gán null
            }
            else
            {
                user.ApartmentId = ApartmentId;
            }
            
            await _userManager.UpdateAsync(user);

            // Lấy vai trò hiện tại
            var userRoles = await _userManager.GetRolesAsync(user);

            // Xóa vai trò không còn chọn
            foreach (var role in userRoles.Except(selectedRoles ?? Array.Empty<string>()))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            // Thêm vai trò mới
            foreach (var role in (selectedRoles ?? Array.Empty<string>()).Except(userRoles))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET:
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Apartment) // include apartment
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST:
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra nếu là tài khoản đang đăng nhập thì không cho phép xóa
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                TempData["Error"] = "Lỗi không tìm thấy người dùng hiện tại.";
                return RedirectToAction(nameof(Delete));
            }
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Delete));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Đã xóa tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("Delete", user); // Quay lại trang Delete để hiển thị lỗi
        }

    }
}