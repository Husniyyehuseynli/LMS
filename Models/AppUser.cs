using Microsoft.AspNetCore.Identity;

namespace LMS.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsInstructor { get; set; } = false;
        public bool IsTeacher { get; set; } = false;

        public List<Enrollment> Enrollments { get; set; }
        public List<QuizResult> QuizResults { get; set; }
    }
}
