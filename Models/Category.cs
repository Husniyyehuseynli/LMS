using LMS.Models.Base;

namespace LMS.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public CategoryIcon Icon { get; set; } = CategoryIcon.Book;
        public List<Course> Courses { get; set; }
    }
}
