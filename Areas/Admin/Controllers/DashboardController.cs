using LMS.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.CourseCount = await _db.Courses.CountAsync(c => !c.IsDeleted);
            ViewBag.CategoryCount = await _db.Categories.CountAsync(c => !c.IsDeleted);
            ViewBag.QuizCount = await _db.Quizzes.CountAsync(q => !q.IsDeleted);
            ViewBag.StudentCount = await _db.Enrollments.Select(e => e.StudentId).Distinct().CountAsync();
            ViewBag.EnrollmentCount = await _db.Enrollments.CountAsync();

            // Average lesson-completion % across every enrollment (courses with
            // no lessons yet are excluded so they don't drag the average to 0).
            var enrollmentsWithLessons = await _db.Enrollments
                .Where(e => !e.IsDeleted)
                .Select(e => new
                {
                    e.StudentId,
                    e.CourseId,
                    TotalLessons = _db.Lessons.Count(l => l.CourseId == e.CourseId && !l.IsDeleted)
                })
                .Where(e => e.TotalLessons > 0)
                .ToListAsync();

            double avgLessonCompletion = 0;
            if (enrollmentsWithLessons.Any())
            {
                var percentages = new List<double>();
                foreach (var e in enrollmentsWithLessons)
                {
                    int completed = await _db.LessonProgresses.CountAsync(p =>
                        p.StudentId == e.StudentId && p.IsCompleted &&
                        p.Lesson.CourseId == e.CourseId && !p.Lesson.IsDeleted);
                    percentages.Add(completed * 100.0 / e.TotalLessons);
                }
                avgLessonCompletion = Math.Round(percentages.Average(), 1);
            }
            ViewBag.AvgLessonCompletion = avgLessonCompletion;

            return View();
        }
    }
}
