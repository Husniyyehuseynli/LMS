using LMS.Models.Base;

namespace LMS.Models
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string InstructorName { get; set; }
        public int DurationHours { get; set; }
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public CourseLanguage Language { get; set; } = CourseLanguage.Azerbaijani;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }

        public Category Category { get; set; }
        public int CategoryId { get; set; }

        // Optional link to a Teacher profile (used for subject teachers, e.g. Entrance Exam Preparation).
        public Teacher? Teacher { get; set; }
        public int? TeacherId { get; set; }

        public List<Quiz> Quizzes { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
