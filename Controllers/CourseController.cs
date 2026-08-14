using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public CourseController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            IQueryable<Course> query = _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Reviews)
                .Include(c => c.Enrollments)
                .Where(c => !c.IsDeleted);

            if (categoryId != null)
            {
                query = query.Where(c => c.CategoryId == categoryId);
            }

            ViewBag.Categories = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;

            List<Course> courses = await query.OrderBy(c => c.Id).ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            Course? course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Quizzes)
                .Include(c => c.Lessons)
                .Include(c => c.Teacher)
                .Include(c => c.Reviews).ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (course == null) return NotFound();

            bool isEnrolled = false;
            bool hasReviewed = false;
            Dictionary<int, bool> lessonProgress = new Dictionary<int, bool>();
            int completedLessonCount = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.GetUserAsync(User);
                isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == course.Id && e.StudentId == user.Id);
                hasReviewed = await _db.Reviews.AnyAsync(r => r.CourseId == course.Id && r.StudentId == user.Id);

                if (isEnrolled)
                {
                    lessonProgress = await _db.LessonProgresses
                        .Where(p => p.StudentId == user.Id && p.Lesson.CourseId == course.Id)
                        .ToDictionaryAsync(p => p.LessonId, p => p.IsCompleted);
                    completedLessonCount = lessonProgress.Count(p => p.Value);
                }
            }
            ViewBag.IsEnrolled = isEnrolled;
            ViewBag.HasReviewed = hasReviewed;
            ViewBag.LessonProgress = lessonProgress;

            int totalLessonCount = course.Lessons?.Count(l => !l.IsDeleted) ?? 0;
            ViewBag.ProgressPercent = totalLessonCount > 0 ? (int)Math.Round(completedLessonCount * 100.0 / totalLessonCount) : 0;

            var activeReviews = course.Reviews?.Where(r => !r.IsDeleted).OrderByDescending(r => r.CreatedDate).ToList() ?? new List<Review>();
            ViewBag.AverageRating = activeReviews.Any() ? Math.Round(activeReviews.Average(r => r.Rating), 1) : 0;
            ViewBag.ReviewCount = activeReviews.Count;
            ViewBag.Reviews = activeReviews;
            ViewBag.StudentCount = await _db.Enrollments.CountAsync(e => e.CourseId == course.Id && !e.IsDeleted);

            // Rating distribution for the "5★ -> 1★" bars shown in the Reviews tab.
            ViewBag.RatingDistribution = Enumerable.Range(1, 5)
                .ToDictionary(star => star, star => activeReviews.Count(r => r.Rating == star));

            // Instructor stats — how many courses this teacher gives and how many
            // students they have across all of them (LINQ over the existing tables,
            // no new model needed).
            if (course.TeacherId != null)
            {
                ViewBag.TeacherCourseCount = await _db.Courses
                    .CountAsync(c => c.TeacherId == course.TeacherId && !c.IsDeleted);
                ViewBag.TeacherStudentCount = await _db.Enrollments
                    .CountAsync(e => e.Course.TeacherId == course.TeacherId && !e.IsDeleted);
            }

            // Related courses: same category or same teacher, current course excluded.
            ViewBag.RelatedCourses = await _db.Courses
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted && c.Id != course.Id &&
                            (c.CategoryId == course.CategoryId || (course.TeacherId != null && c.TeacherId == course.TeacherId)))
                .OrderBy(c => c.Id)
                .Take(4)
                .ToListAsync();

            return View(course);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            bool alreadyEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == user.Id);
            if (!alreadyEnrolled)
            {
                Enrollment enrollment = new Enrollment()
                {
                    CourseId = courseId,
                    StudentId = user.Id
                };
                await _db.Enrollments.AddAsync(enrollment);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = courseId });
        }
    }
}
