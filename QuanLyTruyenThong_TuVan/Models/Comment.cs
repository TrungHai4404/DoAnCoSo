using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Comment
    {
        public int Id { get; set; }


        public string ResidentId { get; set; }
        public ApplicationResident Resident { get; set; }

        [StringLength(100)]
        public string ResidentName { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [Required]
        public CommentType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public CommentStatus Status { get; set; }

        // Quan hệ 1-n với Response
        public ICollection<Response> Responses { get; set; } = new List<Response>();
    }
    public enum CommentStatus
    {
        Submitted,
        Processing,
        Completed,
        Failed
    }
    public enum CommentType
    {
        Hygiene_And_Environment,
        Security_And_Safety,
        Technical_And_Maintenance,
        Shared_Amenities,
        ServiceFees_And_FinancialMatters,
        Rules_And_Regulations,
        StaffAttitude_And_ManagementService,
        Communication_And_Information,
        Other
    }
}
