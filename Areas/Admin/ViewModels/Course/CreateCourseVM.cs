using System.ComponentModel.DataAnnotations;
using LMS.Models;

namespace LMS.Areas.Admin.ViewModels.Course
{
    public record CreateCourseVM
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Short description is required")]
        [StringLength(200, MinimumLength = 5)]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Instructor name is required")]
        public string InstructorName { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 1000, ErrorMessage = "Duration must be between 1 and 1000 hours")]
        public int DurationHours { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        public CourseLevel Level { get; set; }

        [Required(ErrorMessage = "Language is required")]
        public CourseLanguage Language { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile ImageFile { get; set; }

        public string? VideoUrl { get; set; }
    }
}
