using System.ComponentModel.DataAnnotations;

namespace QuanLyTruyenThong_TuVan.Models
{
    public class Forum
    {
        public int Id { get; set; }

        public TypeTopic TypeTopic { get; set; }

        public int PostId { get; set; }
    }
    public enum TypeTopic
    {
        Thông_báo,
	    Tin_tức,
        Hỏi_đáp,
	    Chia_sẻ_kinh_nghiệm,
	    Góp_ý_Phản_hồi,
	    Sự_kiện_Hoạt_động_cộng_đồng,
        // Leen gpt bỏ vô đây
        // load view thì load mấy cái này lên
        // Gán lại dô ForumPost okee
    }
}
