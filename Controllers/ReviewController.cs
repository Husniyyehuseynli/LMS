using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ReviewController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int courseId, int rating, string? comment)
        {
            if (rating < 1) rating = 1;
            if (rating > 5) rating = 5;

            AppUser user = await _userManager.GetUserAsync(User);

            bool isEnrolled = await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == user.Id);
            if (!isEnrolled)
            {
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            Review? existing = await _db.Reviews.FirstOrDefaultAsync(r => r.CourseId == courseId && r.StudentId == user.Id);
            if (existing != null)
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.CreatedDate = DateTime.Now;
            }
            else
            {
                await _db.Reviews.AddAsync(new Review
                {
                    CourseId = courseId,
                    StudentId = user.Id,
                    Rating = rating,
                    Comment = comment
                });
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }
}
