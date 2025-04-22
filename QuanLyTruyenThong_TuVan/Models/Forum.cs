using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Forum
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string TypeTopic { get; set; }

        public int PostId { get; set; }
    }
}
