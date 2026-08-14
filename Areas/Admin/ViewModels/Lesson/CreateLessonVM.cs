using System.ComponentModel.DataAnnotations;

namespace LMS.Areas.Admin.ViewModels.Lesson
{
    public record CreateLessonVM
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; }

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        [Required(ErrorMessage = "Order is required")]
        [Range(1, 500, ErrorMessage = "Order must be between 1 and 500")]
        public int OrderIndex { get; set; }

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
    }
}
