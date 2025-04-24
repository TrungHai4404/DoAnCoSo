using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models.ViewModels;
using QuanLyTruyenThong_TuVan.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace QuanLyTruyenThong_TuVan.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class VoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationResident> _userManager;

        public VoteController(ApplicationDbContext context, UserManager<ApplicationResident> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoteViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);

            var vote = new Vote
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CreatedBy = user.FullName,
                CreatedByUser = user,
                Status = model.Status,
                VoteOptions = model.Options.Select(o => new VoteOption
                {
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    OptionsJson = JsonSerializer.Serialize(
                    o.RawOptions?.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                }).ToList()
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tạo bình chọn thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var votes = _context.Votes.Include(v => v.VoteOptions).ToList();
            return View(votes);
        }
        // Sửa bình chọn
        public async Task<IActionResult> Edit(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null) return NotFound();

            var model = new VoteViewModel
            {
                VoteId = vote.Id,
                Title = vote.Title,
                Description = vote.Description,
                StartDate = vote.StartDate,
                EndDate = vote.EndDate,
                Status = vote.Status,
                Options = vote.VoteOptions.Select(o => new VoteOptionInputModel
                {
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    RawOptions = string.Join("\n", o.Options) // Dùng property `Options` (deserialize từ OptionsJson)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VoteViewModel model, string? DeletedOptionIds)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vote = await _context.Votes.FirstOrDefaultAsync(v => v.Id == model.VoteId);
            if (vote == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin biểu quyết
            vote.Title = model.Title;
            vote.Description = model.Description;
            vote.StartDate = model.StartDate;
            vote.EndDate = model.EndDate;
            vote.Status = model.Status;

            // Xoá các câu hỏi đã bị xóa
            if (!string.IsNullOrEmpty(DeletedOptionIds))
            {
                var deletedIds = DeletedOptionIds.Split(',')
                                     .Where(x => int.TryParse(x, out _))
                                     .Select(int.Parse)
                                     .ToList();

                var optionsToDelete = _context.VoteOptions
                    .Where(o => deletedIds.Contains(o.Id) && o.VoteId == model.VoteId);

                _context.VoteOptions.RemoveRange(optionsToDelete);
            }

            // Cập nhật hoặc thêm mới câu hỏi
            foreach (var opt in model.Options)
            {
                if (opt.Id > 0)
                {
                    var existing = await _context.VoteOptions.FirstOrDefaultAsync(o => o.Id == opt.Id && o.VoteId == model.VoteId);
                    if (existing != null)
                    {
                        existing.QuestionText = opt.QuestionText;
                        existing.QuestionType = opt.QuestionType;
                        existing.OptionsJson = opt.GetOptionsJson();
                    }
                }
                else
                {
                    var newOption = new VoteOption
                    {
                        QuestionText = opt.QuestionText,
                        QuestionType = opt.QuestionType,
                        OptionsJson = opt.GetOptionsJson(),
                        VoteId = model.VoteId
                    };
                    _context.VoteOptions.Add(newOption);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //Xóa bình chọn
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vote = await _context.Votes.FirstOrDefaultAsync(m => m.Id == id);
            if (vote == null) return NotFound();

            return View(vote);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vote = await _context.Votes.FindAsync(id);
            if (vote != null)
            {
                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
