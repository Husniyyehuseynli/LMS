using LMS.Areas.Admin.ViewModels.Quiz;
using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class QuizController : Controller
    {
        private readonly AppDbContext _db;

        public QuizController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Quiz> quizzes = await _db.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .ToListAsync();
            return View(quizzes);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _db.Courses.Where(c => !c.IsDeleted).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuizVM quizVM)
        {
            ViewBag.Courses = await _db.Courses.Where(c => !c.IsDeleted).ToListAsync();
            if (!ModelState.IsValid) return View(quizVM);

            Quiz quiz = new Quiz()
            {
                Title = quizVM.Title,
                Description = quizVM.Description,
                CourseId = quizVM.CourseId
            };
            await _db.Quizzes.AddAsync(quiz);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Quiz? quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();
            quiz.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int? id)
        {
            Quiz? quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();
            quiz.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Courses = await _db.Courses.Where(c => !c.IsDeleted).ToListAsync();
            Quiz? quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            UpdateQuizVM quizVM = new UpdateQuizVM()
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                CourseId = quiz.CourseId
            };
            return View(quizVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateQuizVM quizVM)
        {
            ViewBag.Courses = await _db.Courses.Where(c => !c.IsDeleted).ToListAsync();
            if (!ModelState.IsValid) return View(quizVM);

            Quiz? oldQuiz = await _db.Quizzes.FindAsync(quizVM.Id);
            if (oldQuiz == null) return NotFound();

            oldQuiz.Title = quizVM.Title;
            oldQuiz.Description = quizVM.Description;
            oldQuiz.CourseId = quizVM.CourseId;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Questions(int? id)
        {
            if (id == null) return NotFound();
            Quiz? quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }
    }
}
