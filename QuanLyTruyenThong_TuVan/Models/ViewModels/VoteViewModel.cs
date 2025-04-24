using System.Text.Json;

namespace QuanLyTruyenThong_TuVan.Models.ViewModels
{
    public class VoteViewModel
    {
        public int VoteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VoteStatus Status { get; set; }
        public List<VoteOptionInputModel> Options { get; set; } = new();
        public string? DeletedOptionIds { get; set; }
    }

    public class VoteOptionInputModel
    {
        public int? Id { get; set; } // Null = mới
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string RawOptions { get; set; } // Nhập lựa chọn bằng textarea
        public string GetOptionsJson()
        {
            var list = RawOptions?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(o => o.Trim())
                                  .ToList() ?? new List<string>();
            return JsonSerializer.Serialize(list);
        }
    }
}
