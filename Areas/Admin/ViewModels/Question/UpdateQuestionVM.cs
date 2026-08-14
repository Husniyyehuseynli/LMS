using System.ComponentModel.DataAnnotations;

namespace LMS.Areas.Admin.ViewModels.Question
{
    public record UpdateQuestionVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Option A is required")]
        public string OptionA { get; set; }

        [Required(ErrorMessage = "Option B is required")]
        public string OptionB { get; set; }

        [Required(ErrorMessage = "Option C is required")]
        public string OptionC { get; set; }

        [Required(ErrorMessage = "Option D is required")]
        public string OptionD { get; set; }

        [Required(ErrorMessage = "Correct option is required")]
        public string CorrectOption { get; set; }

        [Required(ErrorMessage = "Quiz is required")]
        public int QuizId { get; set; }
    }
}
