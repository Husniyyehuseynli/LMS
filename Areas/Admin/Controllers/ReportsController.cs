using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        public class CourseReportRow
        {
            public string CourseName { get; set; }
            public string CategoryName { get; set; }
            public int EnrollmentCount { get; set; }
            public int CertificateCount { get; set; }
        }

        public class MonthPoint
        {
            public string Label { get; set; }
            public int Count { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Quizzes.Where(q => !q.IsDeleted))
                .Include(c => c.Lessons.Where(l => !l.IsDeleted))
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var enrollments = await _db.Enrollments
                .Where(e => !e.IsDeleted)
                .Select(e => new { e.StudentId, e.CourseId, e.EnrolledDate })
                .ToListAsync();

            var quizResults = await _db.QuizResults
                .Select(r => new { r.StudentId, r.QuizId, r.CorrectCount, r.TotalCount })
                .ToListAsync();

            var lessonProgress = await _db.LessonProgresses
                .Where(p => p.IsCompleted && !p.IsDeleted && !p.Lesson.IsDeleted)
                .Select(p => new { p.StudentId, p.Lesson.CourseId })
                .ToListAsync();

            
            var courseRows = new List<CourseReportRow>();
            int totalCertificates = 0;

            foreach (var course in courses)
            {
                var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();
                var activeQuizIds = (course.Quizzes ?? new List<Quiz>()).Select(q => q.Id).ToList();
                int totalLessons = course.Lessons?.Count ?? 0;

                int certificateCount = 0;
                foreach (var enr in courseEnrollments)
                {
                    bool quizzesOk = true;
                    foreach (var quizId in activeQuizIds)
                    {
                        var best = quizResults
                            .Where(r => r.StudentId == enr.StudentId && r.QuizId == quizId && r.TotalCount > 0)
                            .OrderByDescending(r => (double)r.CorrectCount / r.TotalCount)
                            .FirstOrDefault();

                        bool passed = best != null && ((double)best.CorrectCount / best.TotalCount) >= 0.6;
                        if (!passed) { quizzesOk = false; break; }
                    }

                    bool lessonsOk = true;
                    if (totalLessons > 0)
                    {
                        int completed = lessonProgress.Count(p => p.StudentId == enr.StudentId && p.CourseId == course.Id);
                        lessonsOk = completed >= totalLessons;
                    }

                    if (quizzesOk && lessonsOk)
                        certificateCount++;
                }

                totalCertificates += certificateCount;

                courseRows.Add(new CourseReportRow
                {
                    CourseName = course.Name,
                    CategoryName = course.Category?.Name ?? "-",
                    EnrollmentCount = courseEnrollments.Count,
                    CertificateCount = certificateCount
                });
            }

            ViewBag.CourseRows = courseRows.OrderByDescending(r => r.EnrollmentCount).ToList();

            
            var monthly = new List<MonthPoint>();
            var today = DateTime.Now;
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                int count = enrollments.Count(e => e.EnrolledDate.Year == monthDate.Year && e.EnrolledDate.Month == monthDate.Month);
                monthly.Add(new MonthPoint { Label = monthDate.ToString("MMM yyyy"), Count = count });
            }
            ViewBag.MonthlyEnrollments = monthly;

            
            var yearly = enrollments
                .GroupBy(e => e.EnrolledDate.Year)
                .OrderBy(g => g.Key)
                .Select(g => new MonthPoint { Label = g.Key.ToString(), Count = g.Count() })
                .ToList();
            ViewBag.YearlyEnrollments = yearly;

        
            ViewBag.TotalStudents = enrollments.Select(e => e.StudentId).Distinct().Count();
            ViewBag.TotalEnrollments = enrollments.Count;
            ViewBag.TotalCertificates = totalCertificates;
            ViewBag.TotalCourses = courses.Count;

            return View();
        }
    }
}
