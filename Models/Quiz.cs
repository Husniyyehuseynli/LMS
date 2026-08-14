using LMS.Models.Base;

namespace LMS.Models
{
    public class Quiz : BaseEntity
    {
        public string Title { get; set; }
        public string? Description { get; set; }

        public Course Course { get; set; }
        public int CourseId { get; set; }

        public List<Question> Questions { get; set; }
        public List<QuizResult> QuizResults { get; set; }
    }
}
