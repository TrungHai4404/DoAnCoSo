using Microsoft.AspNetCore.Identity;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class ApplicationResident : IdentityUser
    {
        public string FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }

        public int? ApartmentId { get; set; } // Khóa ngoại
        public Apartment? Apartment { get; set; } // Navigation property
    }
}
