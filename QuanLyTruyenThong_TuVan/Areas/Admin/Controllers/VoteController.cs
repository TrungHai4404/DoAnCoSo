using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyTruyenThong_TuVan.Data;
using QuanLyTruyenThong_TuVan.Models.ViewModels;
using QuanLyTruyenThong_TuVan.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

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
                    Id = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    RawOptions = string.Join("\n", o.Options) // Dùng property `Options` (deserialize từ OptionsJson)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VoteViewModel model)
        {
            var DeletedOptionIds = model.DeletedOptionIds;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vote = await _context.Votes
                 .Include(v => v.VoteOptions) // Thêm Include để load các câu hỏi hiện có
                 .FirstOrDefaultAsync(v => v.Id == model.VoteId);

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
        public async Task<IActionResult> Delete(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null)
            {
                return NotFound();
            }

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
                    Id = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    RawOptions = o.OptionsJson
                }).ToList()
            };

            return View(model);
        }

        // POST: Votes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa tất cả VoteOptions liên quan
                _context.VoteOptions.RemoveRange(vote.VoteOptions);
                // Xóa Vote
                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete Error: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi khi xóa biểu quyết.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["Success"] = "Biểu quyết đã được xóa thành công.";
            return RedirectToAction(nameof(Index));
        }
        // Xem chi tiết
        public async Task<IActionResult> Details(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null)
            {
                return NotFound();
            }

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
                    Id = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    RawOptions = string.Join("\n", System.Text.Json.JsonSerializer.Deserialize<string[]>(o.OptionsJson)) // Hoặc logic để parse JSON thành danh sách lựa chọn
                }).ToList()
            };

            return View(model);
        }

        // GET: Votes/Vote/5
        [Authorize]
        public async Task<IActionResult> Vote(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null)
            {
                TempData["Error"] = "Không tìm thấy biểu quyết.";
                return RedirectToAction("Index", "Home");
            }

            if (vote.Status != VoteStatus.active || DateTime.Now < vote.StartDate || DateTime.Now > vote.EndDate)
            {
                TempData["Error"] = "Biểu quyết không khả dụng hoặc đã đóng.";
                return RedirectToAction("Index", "Home");
            }

            var resident = await _context.ApplicationResidents
                .FirstOrDefaultAsync(r => r.UserName == User.Identity.Name);
            if (resident == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin cư dân.";
                return RedirectToAction("Index", "Home");
            }

            var hasVoted = await _context.VoteResults
                .AnyAsync(r => r.VoteId == id && r.Resident == resident);
            if (hasVoted)
            {
                TempData["Error"] = "Bạn đã tham gia biểu quyết này.";
                return RedirectToAction("Index", "Home");
            }

            var model = new VoteResponseViewModel
            {
                VoteId = vote.Id,
                Title = vote.Title,
                Description = vote.Description,
                Status = vote.Status,
                Responses = vote.VoteOptions.Select(o => new VoteOptionResponseViewModel
                {
                    VoteOptionId = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    Options = DeserializeOptions(o.OptionsJson)
                }).ToList()
            };

            // Kiểm tra Options trước khi hiển thị
            foreach (var response in model.Responses)
            {
                if (response.QuestionType != QuestionType.Text && (response.Options == null || !response.Options.Any()))
                {
                    TempData["Error"] = $"Câu hỏi '{response.QuestionText}' không có lựa chọn hợp lệ.";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        // POST: Votes/Vote/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(VoteResponseViewModel model)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == model.VoteId);

            if (vote == null)
            {
                TempData["Error"] = "Không tìm thấy biểu quyết.";
                return RedirectToAction("Index", "Home");
            }

            if (vote.Status != VoteStatus.active || DateTime.Now < vote.StartDate || DateTime.Now > vote.EndDate)
            {
                TempData["Error"] = "Biểu quyết không khả dụng hoặc đã đóng.";
                return RedirectToAction("Index", "Home");
            }

            var resident = await _context.ApplicationResidents
                .FirstOrDefaultAsync(r => r.UserName == User.Identity.Name);
            if (resident == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin cư dân.";
                return RedirectToAction("Index", "Home");
            }

            var hasVoted = await _context.VoteResults
                .AnyAsync(r => r.VoteId == model.VoteId && r.Resident == resident);
            if (hasVoted)
            {
                TempData["Error"] = "Bạn đã tham gia biểu quyết này.";
                return RedirectToAction("Index", "Home");
            }

            for (int i = 0; i < model.Responses.Count; i++)
            {
                ModelState.Remove($"Responses[{i}].Options");
                ModelState.Remove($"Responses[{i}].SelectedOption");
                ModelState.Remove($"Responses[{i}].SelectedOptions");
                ModelState.Remove($"Responses[{i}].TextResponse");

                var response = model.Responses[i];
                var voteOption = vote.VoteOptions.FirstOrDefault(o => o.Id == response.VoteOptionId);
                if (voteOption == null)
                {
                    ModelState.AddModelError($"Responses[{i}]", $"Câu hỏi số {i + 1} không hợp lệ.");
                    continue;
                }

                // Gán lại Options từ VoteOptions để tránh binding sai
                response.Options = DeserializeOptions(voteOption.OptionsJson);
                Console.WriteLine($"Response {i} Options after binding: {(response.Options == null ? "null" : string.Join(", ", response.Options))}");

                // Kiểm tra Options cho SingleChoice và MultipleChoice
                if (response.QuestionType != QuestionType.Text && (response.Options == null || !response.Options.Any()))
                {
                    ModelState.AddModelError($"Responses[{i}].Options", $"Câu hỏi số {i + 1}: Yêu cầu ít nhất một lựa chọn.");
                }

                if (response.QuestionType == QuestionType.SingleChoice)
                {
                    if (string.IsNullOrWhiteSpace(response.SelectedOption))
                    {
                        ModelState.AddModelError($"Responses[{i}].SelectedOption", $"Câu hỏi số {i + 1}: Vui lòng chọn một lựa chọn.");
                    }
                    else if (response.Options != null && !response.Options.Contains(response.SelectedOption))
                    {
                        ModelState.AddModelError($"Responses[{i}].SelectedOption", $"Câu hỏi số {i + 1}: Lựa chọn không hợp lệ.");
                    }
                }
                else if (response.QuestionType == QuestionType.Text)
                {
                    if (string.IsNullOrWhiteSpace(response.TextResponse))
                    {
                        ModelState.AddModelError($"Responses[{i}].TextResponse", $"Câu hỏi số {i + 1}: Vui lòng nhập câu trả lời.");
                    }
                }
                // MultipleChoice là tùy chọn
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Any())
                    .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
                Console.WriteLine("ModelState Errors: " + (errors.Any() ? string.Join("; ", errors) : "None"));
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin: " + string.Join("; ", errors.Select(e => e.Split(": ")[1]));
                // Tái tạo Responses từ VoteOptions
                model.Responses = vote.VoteOptions.Select(o => new VoteOptionResponseViewModel
                {
                    VoteOptionId = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    Options = DeserializeOptions(o.OptionsJson),
                    SelectedOption = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOption,
                    SelectedOptions = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOptions,
                    TextResponse = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.TextResponse
                }).ToList();
                return View(model);
            }

            foreach (var response in model.Responses)
            {
                // Kiểm tra VoteOptionId hợp lệ
                var voteOption = await _context.VoteOptions.FindAsync(response.VoteOptionId);
                if (voteOption == null)
                {
                    TempData["Error"] = $"Câu hỏi ID {response.VoteOptionId} không tồn tại.";
                    model.Responses = vote.VoteOptions.Select(o => new VoteOptionResponseViewModel
                    {
                        VoteOptionId = o.Id,
                        QuestionText = o.QuestionText,
                        QuestionType = o.QuestionType,
                        Options = DeserializeOptions(o.OptionsJson),
                        SelectedOption = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOption,
                        SelectedOptions = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOptions,
                        TextResponse = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.TextResponse
                    }).ToList();
                    return View(model);
                }

                if (response.QuestionType == QuestionType.Text && !string.IsNullOrWhiteSpace(response.TextResponse))
                {
                    Console.WriteLine($"Adding VoteResult: VoteId={model.VoteId}, VoteOptionId={response.VoteOptionId}, ResidentId={resident.Id}, Answer={response.TextResponse}");
                    _context.VoteResults.Add(new VoteResult
                    {
                        VoteId = model.VoteId,
                        VoteOptionId = response.VoteOptionId,
                        ResidentId = resident.Id,
                        Resident = resident,
                        Answer = response.TextResponse,
                        VoteAt = DateTime.Now
                    });
                }
                else if (response.QuestionType == QuestionType.SingleChoice && !string.IsNullOrWhiteSpace(response.SelectedOption))
                {
                    Console.WriteLine($"Adding VoteResult: VoteId={model.VoteId}, VoteOptionId={response.VoteOptionId}, ResidentId={resident.Id}, Answer={response.SelectedOption}");
                    _context.VoteResults.Add(new VoteResult
                    {
                        VoteId = model.VoteId,
                        VoteOptionId = response.VoteOptionId,
                        ResidentId = resident.Id,
                        Resident = resident,
                        Answer = response.SelectedOption,
                        VoteAt = DateTime.Now
                    });
                }
                else if (response.QuestionType == QuestionType.MultipleChoice && response.SelectedOptions != null && response.SelectedOptions.Any())
                {
                    foreach (var option in response.SelectedOptions)
                    {
                        Console.WriteLine($"Adding VoteResult: VoteId={model.VoteId}, VoteOptionId={response.VoteOptionId}, ResidentId={resident.Id}, Answer={option}");
                        _context.VoteResults.Add(new VoteResult
                        {
                            VoteId = model.VoteId,
                            VoteOptionId = response.VoteOptionId,
                            ResidentId = resident.Id,
                            Resident = resident,
                            Answer = option,
                            VoteAt = DateTime.Now
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã tham gia biểu quyết!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Vote Submission Error: " + ex.Message);
                TempData["Error"] = "Đã xảy ra lỗi khi gửi câu trả lời: " + ex.Message;
                model.Responses = vote.VoteOptions.Select(o => new VoteOptionResponseViewModel
                {
                    VoteOptionId = o.Id,
                    QuestionText = o.QuestionText,
                    QuestionType = o.QuestionType,
                    Options = DeserializeOptions(o.OptionsJson),
                    SelectedOption = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOption,
                    SelectedOptions = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.SelectedOptions,
                    TextResponse = model.Responses.FirstOrDefault(r => r.VoteOptionId == o.Id)?.TextResponse
                }).ToList();
                return View(model);
            }
        }

        private string[] DeserializeOptions(string optionsJson)
        {
            if (string.IsNullOrEmpty(optionsJson))
            {
                Console.WriteLine("OptionsJson is empty or null");
                return new string[0];
            }

            try
            {
                Console.WriteLine($"OptionsJson: {optionsJson}");
                if (optionsJson.StartsWith("[") && optionsJson.EndsWith("]"))
                {
                    var options = JsonSerializer.Deserialize<string[]>(optionsJson) ?? new string[0];
                    Console.WriteLine($"Deserialized Options: {string.Join(", ", options)}");
                    return options;
                }
                var splitOptions = optionsJson.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToArray();
                Console.WriteLine($"Split Options: {string.Join(", ", splitOptions)}");
                return splitOptions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialize Options Error: {ex.Message}");
                return new string[0];
            }
        }
        // Xem kết quả
        [Authorize]
        public async Task<IActionResult> Results(int id)
        {
            var vote = await _context.Votes
                .Include(v => v.VoteOptions)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vote == null)
            {
                TempData["Error"] = "Không tìm thấy biểu quyết.";
                return RedirectToAction("Index", "Home");
            }

            var voteResults = await _context.VoteResults
                .Where(r => r.VoteId == id)
                .Include(r => r.VoteOption)
                .Include(r => r.Resident) // Thêm Resident để lấy thông tin cư dân
                .ToListAsync();

            var totalParticipants = voteResults
                .Select(r => r.ResidentId)
                .Distinct()
                .Count();

            var model = new VoteResultStatisticsViewModel
            {
                VoteId = vote.Id,
                Title = vote.Title,
                Description = vote.Description,
                TotalParticipants = totalParticipants
            };

            foreach (var option in vote.VoteOptions)
            {
                var optionResults = voteResults
                    .Where(r => r.VoteOptionId == option.Id)
                    .ToList();

                var optionStats = new VoteOptionStatisticsViewModel
                {
                    VoteOptionId = option.Id,
                    QuestionText = option.QuestionText,
                    QuestionType = option.QuestionType
                };

                if (option.QuestionType == QuestionType.Text)
                {
                    optionStats.TextAnswers = optionResults
                        .Select(r => new TextAnswerViewModel
                        {
                            ResidentId = r.ResidentId,
                            ResidentName = r.Resident?.FullName ?? r.ResidentId,
                            Answer = r.Answer,
                            VoteAt = r.VoteAt
                        })
                        .ToList();
                }
                else // SingleChoice or MultipleChoice
                {
                    var answerGroups = optionResults
                        .GroupBy(r => r.Answer)
                        .Select(g => new
                        {
                            Answer = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    var totalAnswers = optionResults.Count;
                    optionStats.AnswerStatistics = answerGroups
                        .Select(g => new AnswerStatisticsViewModel
                        {
                            Answer = g.Answer,
                            Count = g.Count,
                            Percentage = totalAnswers > 0 ? Math.Round((double)g.Count / totalAnswers * 100, 2) : 0
                        })
                        .ToList();

                    // Thêm các lựa chọn chưa được chọn
                    var options = DeserializeOptions(option.OptionsJson);
                    foreach (var opt in options)
                    {
                        if (!optionStats.AnswerStatistics.Any(a => a.Answer == opt))
                        {
                            optionStats.AnswerStatistics.Add(new AnswerStatisticsViewModel
                            {
                                Answer = opt,
                                Count = 0,
                                Percentage = 0
                            });
                        }
                    }
                }

                model.OptionStatistics.Add(optionStats);
            }

            return View(model);
        }



    }
}
