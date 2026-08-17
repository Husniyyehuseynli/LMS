using LMS.Models;

namespace LMS.Utilites
{
    public static class CategoryIconExtension
    {
   
        public static string ToFaClass(this CategoryIcon icon)
        {
            return icon switch
            {
                CategoryIcon.Code => "fa-code",
                CategoryIcon.Design => "fa-palette",
                CategoryIcon.Business => "fa-briefcase",
                CategoryIcon.Marketing => "fa-bullhorn",
                CategoryIcon.Cloud => "fa-cloud",
                CategoryIcon.Data => "fa-chart-line",
                CategoryIcon.Language => "fa-language",
                CategoryIcon.Music => "fa-music",
                CategoryIcon.Health => "fa-heart-pulse",
                CategoryIcon.Camera => "fa-camera",
                CategoryIcon.Chart => "fa-chart-pie",
                _ => "fa-book-open"
            };
        }
    }
}
