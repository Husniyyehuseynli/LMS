using LMS.Models.Base;

namespace LMS.Models
{
    public class Teacher : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
        public string Bio { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Email { get; set; }

        // Optional link to a login account so the teacher can sign in and chat with students.
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        public List<Course> Courses { get; set; }
        public List<TeacherReview> Reviews { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
