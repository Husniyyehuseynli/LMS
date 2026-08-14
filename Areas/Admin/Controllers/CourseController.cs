using LMS.Areas.Admin.ViewModels.Course;
using LMS.DAL;
using LMS.Models;
using LMS.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CourseController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Course> courses = await _db.Courses
                .Include(c => c.Category)
                .ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseVM courseVM)
        {
            ViewBag.Categories = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();

            if (courseVM.ImageFile is null)
            {
                ModelState.AddModelError(nameof(courseVM.ImageFile), "Image is required");
                return View(courseVM);
            }
            if (!courseVM.ImageFile.ContentType.Contains("image/"))
            {
                ModelState.AddModelError(nameof(courseVM.ImageFile), "File must be an image");
                return View(courseVM);
            }
            if (courseVM.ImageFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(courseVM.ImageFile), "File size can not exceed 2MB");
                return View(courseVM);
            }
            if (!ModelState.IsValid) return View(courseVM);

            Course course = new Course()
            {
                Name = courseVM.Name,
                ShortDescription = courseVM.ShortDescription,
                Description = courseVM.Description,
                InstructorName = courseVM.InstructorName,
                DurationHours = courseVM.DurationHours,
                CategoryId = courseVM.CategoryId,
                Level = courseVM.Level,
                Language = courseVM.Language,
                ImageUrl = courseVM.ImageFile.SaveImage(_env, "uploads/courses"),
                VideoUrl = courseVM.VideoUrl.ToEmbedUrl()
            };

            await _db.Courses.AddAsync(course);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Course? course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            course.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int? id)
        {
            Course? course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();
            course.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();
            Course? course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            UpdateCourseVM courseVM = new UpdateCourseVM()
            {
                Id = course.Id,
                Name = course.Name,
                ShortDescription = course.ShortDescription,
                Description = course.Description,
                InstructorName = course.InstructorName,
                DurationHours = course.DurationHours,
                CategoryId = course.CategoryId,
                Level = course.Level,
                Language = course.Language,
                ImageUrl = course.ImageUrl,
                VideoUrl = course.VideoUrl
            };
            return View(courseVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCourseVM courseVM)
        {            ViewBag.Categories = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();

            if (courseVM.ImageFile != null)
            {
                if (!courseVM.ImageFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError(nameof(courseVM.ImageFile), "File must be an image");
                    return View(courseVM);
                }
                if (courseVM.ImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(courseVM.ImageFile), "File size can not exceed 2MB");
                    return View(courseVM);
                }
            }
            if (!ModelState.IsValid) return View(courseVM);

            Course? oldCourse = await _db.Courses.FindAsync(courseVM.Id);
            if (oldCourse == null) return NotFound();

            oldCourse.Name = courseVM.Name;
            oldCourse.ShortDescription = courseVM.ShortDescription;
            oldCourse.Description = courseVM.Description;
            oldCourse.InstructorName = courseVM.InstructorName;
            oldCourse.DurationHours = courseVM.DurationHours;
            oldCourse.CategoryId = courseVM.CategoryId;
            oldCourse.Level = courseVM.Level;
            oldCourse.Language = courseVM.Language;
            oldCourse.VideoUrl = courseVM.VideoUrl.ToEmbedUrl();

            if (courseVM.ImageFile != null)
            {
                oldCourse.ImageUrl = courseVM.ImageFile.SaveImage(_env, "uploads/courses");
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Manage lessons belonging to a course
        public async Task<IActionResult> Lessons(int? id)
        {
            if (id == null) return NotFound();
            Course? course = await _db.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        // Per-course, per-student lesson-completion report — one row per
        // enrolled student, one column per active lesson, plus an overall %.
        public async Task<IActionResult> Progress(int? id)
        {
            if (id == null) return NotFound();

            Course? course = await _db.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            List<Lesson> activeLessons = course.Lessons?
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.OrderIndex)
                .ToList() ?? new List<Lesson>();

            List<Enrollment> enrollments = await _db.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == id && !e.IsDeleted)
                .OrderBy(e => e.Student.Name)
                .ToListAsync();

            List<LessonProgress> progressRows = await _db.LessonProgresses
                .Where(p => p.Lesson.CourseId == id && !p.Lesson.IsDeleted)
                .ToListAsync();

            // studentId -> (lessonId -> isCompleted), for O(1) lookups in the view.
            Dictionary<string, Dictionary<int, bool>> progressByStudent = enrollments
                .Select(e => e.StudentId)
                .Distinct()
                .ToDictionary(
                    studentId => studentId,
                    studentId => progressRows
                        .Where(p => p.StudentId == studentId)
                        .ToDictionary(p => p.LessonId, p => p.IsCompleted));

            ViewBag.Course = course;
            ViewBag.Lessons = activeLessons;
            ViewBag.ProgressByStudent = progressByStudent;

            return View(enrollments);
        }
    }
}
