using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            AppUser user = await _userManager.GetUserAsync(User);

            List<Enrollment> enrollments = await _db.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .Include(e => e.Course.Quizzes)
                .Include(e => e.Course.Lessons)
                .Where(e => e.StudentId == user.Id && !e.IsDeleted)
                .ToListAsync();

            List<QuizResult> results = await _db.QuizResults
                .Include(r => r.Quiz)
                .Where(r => r.StudentId == user.Id)
                .OrderByDescending(r => r.TakenDate)
                .ToListAsync();

            ViewBag.QuizResults = results;


            List<int> courseIds = enrollments.Select(e => e.CourseId).ToList();
            Dictionary<int, int> completedByCourse = await _db.LessonProgresses
                .Where(p => p.StudentId == user.Id && p.IsCompleted && courseIds.Contains(p.Lesson.CourseId))
                .GroupBy(p => p.Lesson.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseId, x => x.Count);

            ViewBag.CompletedLessonsByCourse = completedByCourse;

            return View(enrollments);
        }

   
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeachingProgress()
        {
            AppUser user = await _userManager.GetUserAsync(User);

            Teacher? teacher = await _db.Teachers
                .Include(t => t.Courses).ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(t => t.AppUserId == user.Id && !t.IsDeleted);

            List<Course> courses = teacher?.Courses?.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList()
                ?? new List<Course>();

            var courseSummaries = new List<TeachingCourseSummary>();
            foreach (var course in courses)
            {
                List<Lesson> activeLessons = course.Lessons?.Where(l => !l.IsDeleted).OrderBy(l => l.OrderIndex).ToList()
                    ?? new List<Lesson>();

                List<Enrollment> enrollmentsForCourse = await _db.Enrollments
                    .Include(e => e.Student)
                    .Where(e => e.CourseId == course.Id && !e.IsDeleted)
                    .OrderBy(e => e.Student.Name)
                    .ToListAsync();

                List<LessonProgress> progressRows = activeLessons.Any()
                    ? await _db.LessonProgresses
                        .Where(p => p.Lesson.CourseId == course.Id && !p.Lesson.IsDeleted)
                        .ToListAsync()
                    : new List<LessonProgress>();

                var students = enrollmentsForCourse.Select(e =>
                {
                    int completed = progressRows.Count(p => p.StudentId == e.StudentId && p.IsCompleted);
                    int percent = activeLessons.Any() ? (int)Math.Round(completed * 100.0 / activeLessons.Count) : 0;
                    return new TeachingStudentRow
                    {
                        Name = $"{e.Student.Name} {e.Student.Surname}",
                        Email = e.Student.Email ?? "",
                        Percent = percent
                    };
                }).ToList();

                courseSummaries.Add(new TeachingCourseSummary
                {
                    CourseId = course.Id,
                    CourseName = course.Name,
                    LessonCount = activeLessons.Count,
                    Students = students
                });
            }

            ViewBag.TeacherName = teacher?.FullName ?? user.Name;
            return View(courseSummaries);
        }
    }


    public class TeachingCourseSummary
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public int LessonCount { get; set; }
        public List<TeachingStudentRow> Students { get; set; } = new();
    }

    public class TeachingStudentRow
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public int Percent { get; set; }
    }
}
