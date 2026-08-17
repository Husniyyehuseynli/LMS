using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class CertificateController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly NotificationService _notifier;

        public CertificateController(AppDbContext db, UserManager<AppUser> userManager, NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _notifier = notifier;
        }

        public async Task<IActionResult> View(int courseId)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            Course? course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Quizzes)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);

            if (course == null) return NotFound();

            bool isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == user.Id);
            if (!isEnrolled)
            {
                TempData["CertificateError"] = "You need to enroll in this course first.";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            var activeQuizzes = course.Quizzes?.Where(q => !q.IsDeleted).ToList() ?? new List<Quiz>();

            if (activeQuizzes.Any())
            {
                List<QuizResult> results = await _db.QuizResults
                    .Where(r => r.StudentId == user.Id && activeQuizzes.Select(q => q.Id).Contains(r.QuizId))
                    .OrderByDescending(r => r.TakenDate)
                    .ToListAsync();

                foreach (var quiz in activeQuizzes)
                {
                    QuizResult? best = results
                        .Where(r => r.QuizId == quiz.Id)
                        .OrderByDescending(r => r.TotalCount == 0 ? 0 : (double)r.CorrectCount / r.TotalCount)
                        .FirstOrDefault();

                    bool passed = best != null && best.TotalCount > 0 && ((double)best.CorrectCount / best.TotalCount) >= 0.6;
                    if (!passed)
                    {
                        TempData["CertificateError"] = "You need to pass all quizzes in this course (60% or higher) to unlock your certificate.";
                        return RedirectToAction("Details", "Course", new { id = courseId });
                    }
                }
            }

       
            int totalLessons = course.Lessons?.Count(l => !l.IsDeleted) ?? 0;
            if (totalLessons > 0)
            {
                int completedLessons = await _db.LessonProgresses
                    .CountAsync(p => p.StudentId == user.Id && p.IsCompleted &&
                                      p.Lesson.CourseId == courseId && !p.Lesson.IsDeleted);

                if (completedLessons < totalLessons)
                {
                    TempData["CertificateError"] = "You need to complete all lessons in this course to unlock your certificate.";
                    return RedirectToAction("Details", "Course", new { id = courseId });
                }
            }

            ViewBag.Course = course;
            ViewBag.StudentName = $"{user.Name} {user.Surname}".Trim();
            ViewBag.IssueDate = DateTime.Now;

        
            string certUrl = $"/Certificate/View?courseId={courseId}";
            bool alreadyNotified = await _db.Notifications.AnyAsync(n =>
                n.RecipientId == user.Id && n.Url == certUrl && !n.IsDeleted);
            if (!alreadyNotified)
            {
                await _notifier.NotifyAsync(user.Id,
                    "Sertifikat hazırdır!",
                    $"Your certificate for \"{course.Name}\" is ready!", certUrl, "🏆");
                await _db.SaveChangesAsync();
            }

            return View();
        }
    }
}
