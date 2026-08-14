using LMS.Models.Base;

namespace LMS.Models
{
    public class Question : BaseEntity
    {
        public string Text { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }

        // Stores "A", "B", "C" or "D"
        public string CorrectOption { get; set; }

        public Quiz Quiz { get; set; }
        public int QuizId { get; set; }
    }
}
