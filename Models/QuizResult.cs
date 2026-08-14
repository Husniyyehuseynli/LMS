using LMS.Models.Base;

namespace LMS.Models
{
    public class QuizResult : BaseEntity
    {
        public AppUser Student { get; set; }
        public string StudentId { get; set; }

        public Quiz Quiz { get; set; }
        public int QuizId { get; set; }

        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime TakenDate { get; set; } = DateTime.Now;
    }
}
