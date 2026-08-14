using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public QuizController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Take(int? id)
        {
            if (id == null) return NotFound();

            AppUser user = await _userManager.GetUserAsync(User);

            Quiz? quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && !q.IsDeleted);

            if (quiz == null) return NotFound();

            bool isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == quiz.CourseId && e.StudentId == user.Id);
            if (!isEnrolled)
            {
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }

            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int quizId, Dictionary<int, string> answers)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            Quiz? quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return NotFound();

            int correctCount = 0;
            foreach (Question question in quiz.Questions)
            {
                if (answers != null && answers.TryGetValue(question.Id, out string? selected))
                {
                    if (!string.IsNullOrEmpty(selected) && selected.Equals(question.CorrectOption, StringComparison.OrdinalIgnoreCase))
                    {
                        correctCount++;
                    }
                }
            }

            QuizResult result = new QuizResult()
            {
                QuizId = quiz.Id,
                StudentId = user.Id,
                CorrectCount = correctCount,
                TotalCount = quiz.Questions.Count
            };

            await _db.QuizResults.AddAsync(result);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = result.Id });
        }

        public async Task<IActionResult> Result(int? id)
        {
            if (id == null) return NotFound();

            QuizResult? result = await _db.QuizResults
                .Include(r => r.Quiz)
                .ThenInclude(q => q.Course)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null) return NotFound();

            // A result belongs to exactly one student — make sure the person
            // viewing it is that student, not someone who guessed the URL.
            AppUser user = await _userManager.GetUserAsync(User);
            if (result.StudentId != user.Id) return Forbid();

            return View(result);
        }
    }
}
