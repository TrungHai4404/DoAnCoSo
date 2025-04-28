using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Vote
    {
        public int Id { get; set; }
        [StringLength(255)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string CreatedBy { get; set; }
        public ApplicationResident CreatedByUser { get; set; }
        public VoteStatus Status { get; set; }

        public ICollection<VoteOption> VoteOptions { get; set; }
        public ICollection<VoteResult> VoteResults { get; set; }

    }
    public enum VoteStatus
    {
        active,
        draft,
        close
    }
}
