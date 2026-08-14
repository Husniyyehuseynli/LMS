using System.ComponentModel.DataAnnotations;
using LMS.Models;

namespace LMS.Areas.Admin.ViewModels.Category
{
    public record CreateCategoryVM
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Icon is required")]
        public CategoryIcon Icon { get; set; }
    }
}
