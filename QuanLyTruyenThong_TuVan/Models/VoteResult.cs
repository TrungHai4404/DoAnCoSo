namespace QuanLyTruyenThong_TuVan.Models
{
    public class VoteResult
    {
        public int Id { get; set; }

        public int VoteId { get; set; }
        public Vote Vote { get; set; }

        public int VoteOptionId { get; set; }
        public VoteOption VoteOption { get; set; }
        public string ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }
        public string Answer { get; set; }
        public DateTime VoteAt { get; set; } = DateTime.Now;
    }
}
