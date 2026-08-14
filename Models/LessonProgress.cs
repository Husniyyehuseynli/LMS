using LMS.Models.Base;

namespace LMS.Models
{
    // One row = one student's completion status for one lesson.
    // The overall "% complete" for a course is calculated on the fly
    // (completed rows / total lessons) instead of being stored, so it
    // never goes stale when lessons are added or removed later.
    public class LessonProgress : BaseEntity
    {
        public AppUser Student { get; set; }
        public string StudentId { get; set; }

        public Lesson Lesson { get; set; }
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
