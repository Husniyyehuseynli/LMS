using LMS.Models.Base;

namespace LMS.Models
{
    public class Lesson : BaseEntity
    {
        public string Title { get; set; }
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }

        // Determines the display order of lessons within a course (1, 2, 3...).
        public int OrderIndex { get; set; }

        public Course Course { get; set; }
        public int CourseId { get; set; }
    }
}
