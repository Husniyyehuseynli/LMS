using LMS.Models.Base;

namespace LMS.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class TeacherApplication : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Bio { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    }
}
