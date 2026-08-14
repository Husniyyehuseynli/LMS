using LMS.Models.Base;

namespace LMS.Models
{
    public class Enrollment : BaseEntity
    {
        public AppUser Student { get; set; }
        public string StudentId { get; set; }

        public Course Course { get; set; }
        public int CourseId { get; set; }

        public DateTime EnrolledDate { get; set; } = DateTime.Now;
    }
}
