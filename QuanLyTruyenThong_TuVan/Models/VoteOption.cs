using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class VoteOption
    {
        public int Id { get; set; }

        public int VoteId { get; set; }
        public Vote Vote { get; set; }

        public string QuestionText { get; set; }

        [StringLength(50)]
        public string QuestionType { get; set; }

        public string Options { get; set; }  // JSON string
    }
}
