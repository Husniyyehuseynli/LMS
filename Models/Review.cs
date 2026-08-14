using LMS.Models.Base;

namespace LMS.Models
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public AppUser Student { get; set; }
        public string StudentId { get; set; }

        public Course Course { get; set; }
        public int CourseId { get; set; }
    }
}
