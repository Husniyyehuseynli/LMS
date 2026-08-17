using LMS.Areas.Admin.ViewModels.Lesson;
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
    public class LessonController : Controller
    {
        private readonly AppDbContext _db;

        public LessonController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Create(int courseId)
        {
            Course? course = await _db.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            
            int nextOrder = await _db.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => (int?)l.OrderIndex)
                .MaxAsync() ?? 0;

            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.Name;
            return View(new CreateLessonVM { CourseId = courseId, OrderIndex = nextOrder + 1 });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLessonVM lessonVM)
        {
            if (!ModelState.IsValid)
            {
                Course? c = await _db.Courses.FindAsync(lessonVM.CourseId);
                ViewBag.CourseId = lessonVM.CourseId;
                ViewBag.CourseName = c?.Name;
                return View(lessonVM);
            }

            Lesson lesson = new Lesson()
            {
                Title = lessonVM.Title,
                Content = lessonVM.Content,
                VideoUrl = lessonVM.VideoUrl.ToEmbedUrl(),
                OrderIndex = lessonVM.OrderIndex,
                CourseId = lessonVM.CourseId
            };

            await _db.Lessons.AddAsync(lesson);
            await _db.SaveChangesAsync();
            return RedirectToAction("Lessons", "Course", new { id = lessonVM.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Lesson? lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            int courseId = lesson.CourseId;
            lesson.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction("Lessons", "Course", new { id = courseId });
        }

        public async Task<IActionResult> Update(int? id)
        {
            Lesson? lesson = await _db.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            UpdateLessonVM lessonVM = new UpdateLessonVM()
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                OrderIndex = lesson.OrderIndex,
                CourseId = lesson.CourseId
            };
            return View(lessonVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateLessonVM lessonVM)
        {
            if (!ModelState.IsValid) return View(lessonVM);

            Lesson? oldLesson = await _db.Lessons.FindAsync(lessonVM.Id);
            if (oldLesson == null) return NotFound();

            oldLesson.Title = lessonVM.Title;
            oldLesson.Content = lessonVM.Content;
            oldLesson.VideoUrl = lessonVM.VideoUrl.ToEmbedUrl();
            oldLesson.OrderIndex = lessonVM.OrderIndex;

            await _db.SaveChangesAsync();
            return RedirectToAction("Lessons", "Course", new { id = oldLesson.CourseId });
        }
    }
}
