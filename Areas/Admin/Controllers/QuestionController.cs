using LMS.Areas.Admin.ViewModels.Question;
using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _db;

        public QuestionController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Create(int quizId)
        {
            Quiz? quiz = await _db.Quizzes.FindAsync(quizId);
            if (quiz == null) return NotFound();

            ViewBag.QuizId = quizId;
            ViewBag.QuizTitle = quiz.Title;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestionVM questionVM)
        {
            if (!ModelState.IsValid)
            {
                Quiz? q = await _db.Quizzes.FindAsync(questionVM.QuizId);
                ViewBag.QuizId = questionVM.QuizId;
                ViewBag.QuizTitle = q?.Title;
                return View(questionVM);
            }

            Question question = new Question()
            {
                Text = questionVM.Text,
                OptionA = questionVM.OptionA,
                OptionB = questionVM.OptionB,
                OptionC = questionVM.OptionC,
                OptionD = questionVM.OptionD,
                CorrectOption = questionVM.CorrectOption.ToUpper(),
                QuizId = questionVM.QuizId
            };

            await _db.Questions.AddAsync(question);
            await _db.SaveChangesAsync();
            return RedirectToAction("Questions", "Quiz", new { id = questionVM.QuizId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Question? question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound();
            int quizId = question.QuizId;
            question.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction("Questions", "Quiz", new { id = quizId });
        }

        public async Task<IActionResult> Update(int? id)
        {
            Question? question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound();

            UpdateQuestionVM questionVM = new UpdateQuestionVM()
            {
                Id = question.Id,
                Text = question.Text,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectOption = question.CorrectOption,
                QuizId = question.QuizId
            };
            return View(questionVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateQuestionVM questionVM)
        {
            if (!ModelState.IsValid) return View(questionVM);

            Question? oldQuestion = await _db.Questions.FindAsync(questionVM.Id);
            if (oldQuestion == null) return NotFound();

            oldQuestion.Text = questionVM.Text;
            oldQuestion.OptionA = questionVM.OptionA;
            oldQuestion.OptionB = questionVM.OptionB;
            oldQuestion.OptionC = questionVM.OptionC;
            oldQuestion.OptionD = questionVM.OptionD;
            oldQuestion.CorrectOption = questionVM.CorrectOption.ToUpper();

            await _db.SaveChangesAsync();
            return RedirectToAction("Questions", "Quiz", new { id = oldQuestion.QuizId });
        }
    }
}
