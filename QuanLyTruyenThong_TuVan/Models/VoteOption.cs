using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class VoteOption
    {
        public int Id { get; set; }
        public int VoteId { get; set; }
        public Vote Vote { get; set; }

        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }

        public string OptionsJson { get; set; }

        [NotMapped]
        public List<string> Options
        {
            get => string.IsNullOrWhiteSpace(OptionsJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(OptionsJson);
            set => OptionsJson = JsonSerializer.Serialize(value);
        }
        public ICollection<VoteResult> VoteResults { get; set; }
    }
    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        Text
    }

}
