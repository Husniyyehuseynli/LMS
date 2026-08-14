using System.ComponentModel.DataAnnotations;

namespace LMS.Areas.Admin.ViewModels.Teacher
{
    public class CreateTeacherVM
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Bio { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public IFormFile? PhotoFile { get; set; }
    }
}
