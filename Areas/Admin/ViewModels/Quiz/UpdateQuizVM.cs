using System.ComponentModel.DataAnnotations;

namespace LMS.Areas.Admin.ViewModels.Quiz
{
    public record UpdateQuizVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
    }
}
