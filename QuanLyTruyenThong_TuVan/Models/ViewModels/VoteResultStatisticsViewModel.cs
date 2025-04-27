using QuanLyTruyenThong_TuVan.Models;
using System;
using System.Collections.Generic;

namespace QuanLyTruyenThong_TuVan.Models.ViewModels
{
    public class VoteResultStatisticsViewModel
    {
        public int VoteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TotalParticipants { get; set; }
        public List<VoteOptionStatisticsViewModel> OptionStatistics { get; set; } = new List<VoteOptionStatisticsViewModel>();
    }

    public class VoteOptionStatisticsViewModel
    {
        public int VoteOptionId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<AnswerStatisticsViewModel> AnswerStatistics { get; set; } = new List<AnswerStatisticsViewModel>();
        public List<TextAnswerViewModel> TextAnswers { get; set; } = new List<TextAnswerViewModel>();
    }

    public class AnswerStatisticsViewModel
    {
        public string Answer { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TextAnswerViewModel
    {
        public string ResidentId { get; set; }
        public string ResidentName { get; set; }
        public string Answer { get; set; }
        public DateTime VoteAt { get; set; }
    }
}