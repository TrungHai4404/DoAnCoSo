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

        public int CreatedBy { get; set; }
        public ApplicationResident CreatedByUser { get; set; }

        [StringLength(50)]
        public string Status { get; set; }
    }
}
