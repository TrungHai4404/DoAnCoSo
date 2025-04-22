namespace QuanLyTruyenThong_TuVan.Models
{
    public class VoteResult
    {
        public int Id { get; set; }

        public int VoteId { get; set; }
        public Vote Vote { get; set; }

        public int VoteOptionId { get; set; }
        public VoteOption VoteOption { get; set; }

        public int ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }

        public DateTime VoteAt { get; set; } = DateTime.Now;
    }
}
