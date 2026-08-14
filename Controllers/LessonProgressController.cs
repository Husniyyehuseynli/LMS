using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class LessonProgressController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly NotificationService _notifier;

        public LessonProgressController(AppDbContext db, UserManager<AppUser> userManager, NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _notifier = notifier;
        }

        // Flips a lesson between "completed" / "not completed" for the current
        // student. Only enrolled students can mark progress on a lesson.
        [HttpPost]
        public async Task<IActionResult> Toggle(int lessonId)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            Lesson? lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);
            if (lesson == null) return NotFound();

            bool isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == lesson.CourseId && e.StudentId == user.Id);
            if (!isEnrolled) return Forbid();

            LessonProgress? progress = await _db.LessonProgresses
                .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.StudentId == user.Id);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    LessonId = lessonId,
                    StudentId = user.Id,
                    IsCompleted = true,
                    CompletedDate = DateTime.Now
                };
                await _db.LessonProgresses.AddAsync(progress);
            }
            else
            {
                progress.IsCompleted = !progress.IsCompleted;
                progress.CompletedDate = progress.IsCompleted ? DateTime.Now : null;
            }

            await _db.SaveChangesAsync();

            // Recalculate the course-wide percentage so the JS can update
            // both the lesson icon and the progress bar in one round trip.
            int totalLessons = await _db.Lessons.CountAsync(l => l.CourseId == lesson.CourseId && !l.IsDeleted);
            int completedLessons = await _db.LessonProgresses
                .CountAsync(p => p.StudentId == user.Id && p.IsCompleted &&
                                  p.Lesson.CourseId == lesson.CourseId && !p.Lesson.IsDeleted);

            int percent = totalLessons > 0 ? (int)Math.Round(completedLessons * 100.0 / totalLessons) : 0;

            // Notify the student (and the course's teacher, if any) the first
            // time all lessons in the course are completed. Guarded by Url so
            // re-toggling a lesson off/on later doesn't spam duplicates.
            if (progress.IsCompleted && percent == 100 && totalLessons > 0)
            {
                string courseUrl = $"/Course/Details/{lesson.CourseId}";
                bool alreadyNotified = await _db.Notifications.AnyAsync(n =>
                    n.RecipientId == user.Id && n.Url == courseUrl && !n.IsDeleted);

                if (!alreadyNotified)
                {
                    Course? course = await _db.Courses
                        .Include(c => c.Teacher)
                        .FirstOrDefaultAsync(c => c.Id == lesson.CourseId);

                    if (course != null)
                    {
                        await _notifier.NotifyAsync(user.Id,
                            "Kurs tamamlandı!",
                            $"Congratulations! You completed every lesson in \"{course.Name}\". Check whether your certificate is ready.",
                            courseUrl, "🎓");

                        if (!string.IsNullOrEmpty(course.Teacher?.AppUserId))
                        {
                            await _notifier.NotifyAsync(course.Teacher.AppUserId,
                                "Tələbə kursu bitirdi",
                                $"{user.Name} {user.Surname} completed every lesson in your course \"{course.Name}\".",
                                "/Dashboard/TeachingProgress", "👏");
                        }

                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Json(new
            {
                lessonId = lesson.Id,
                isCompleted = progress.IsCompleted,
                completedLessons,
                totalLessons,
                percent
            });
        }
    }
}
