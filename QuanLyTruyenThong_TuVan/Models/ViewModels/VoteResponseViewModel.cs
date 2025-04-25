using System.Collections.Generic;
using QuanLyTruyenThong_TuVan.Models;

namespace QuanLyTruyenThong_TuVan.Models.ViewModels
{
    public class VoteResponseViewModel
    {
        public int VoteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public VoteStatus Status { get; set; }
        public List<VoteOptionResponseViewModel> Responses { get; set; } = new List<VoteOptionResponseViewModel>();
    }

    public class VoteOptionResponseViewModel
    {
        public int VoteOptionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public string[] Options { get; set; }
        public string SelectedOption { get; set; } // For SingleChoice
        public string[] SelectedOptions { get; set; } // For MultipleChoice
        public string TextResponse { get; set; } // For Text
    }
}