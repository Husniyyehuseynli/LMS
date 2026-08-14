using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public TeacherController(AppDbContext db, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string? subject)
        {
            IQueryable<Teacher> query = _db.Teachers
                .Include(t => t.Courses)
                .Include(t => t.Reviews)
                .Where(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(subject))
            {
                query = query.Where(t => t.Subject == subject);
            }

            ViewBag.Subjects = await _db.Teachers
                .Where(t => !t.IsDeleted)
                .Select(t => t.Subject)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            ViewBag.SelectedSubject = subject;

            ViewBag.PopularCourses = await _db.Courses
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Id)
                .Take(6)
                .Select(c => new { c.Id, c.Name, c.InstructorName })
                .ToListAsync();

            List<Teacher> teachers = await query
                .OrderByDescending(t => t.FirstName == "Husniyya" && t.LastName == "Huseynli")
                .ThenBy(t => t.Id)
                .ToListAsync();
            return View(teachers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            Teacher? teacher = await _db.Teachers
                .Include(t => t.Courses).ThenInclude(c => c.Category)
                .Include(t => t.Reviews).ThenInclude(r => r.Student)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (teacher == null) return NotFound();

            var activeReviews = teacher.Reviews?.Where(r => !r.IsDeleted).OrderByDescending(r => r.CreatedDate).ToList() ?? new List<TeacherReview>();
            ViewBag.AverageRating = activeReviews.Any() ? Math.Round(activeReviews.Average(r => r.Rating), 1) : 5;
            ViewBag.ReviewCount = activeReviews.Count;
            ViewBag.Reviews = activeReviews;

            bool canMessage = false;
            bool hasReviewed = false;
            bool isStudent = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                AppUser user = await _userManager.GetUserAsync(User);
                isStudent = await _userManager.IsInRoleAsync(user, "Student");
                canMessage = isStudent && !string.IsNullOrEmpty(teacher.AppUserId) && teacher.AppUserId != user.Id;
                hasReviewed = await _db.TeacherReviews.AnyAsync(r => r.TeacherId == teacher.Id && r.StudentId == user.Id);
            }
            ViewBag.CanMessage = canMessage;
            ViewBag.HasReviewed = hasReviewed;
            ViewBag.IsStudent = isStudent;

            return View(teacher);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> Review(int teacherId, int rating, string? comment)
        {
            if (rating < 1) rating = 1;
            if (rating > 5) rating = 5;

            AppUser user = await _userManager.GetUserAsync(User);

            TeacherReview? existing = await _db.TeacherReviews.FirstOrDefaultAsync(r => r.TeacherId == teacherId && r.StudentId == user.Id);
            if (existing != null)
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.CreatedDate = DateTime.Now;
            }
            else
            {
                await _db.TeacherReviews.AddAsync(new TeacherReview
                {
                    TeacherId = teacherId,
                    StudentId = user.Id,
                    Rating = rating,
                    Comment = comment
                });
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = teacherId });
        }

        // Lets an already signed-in Teacher switch into any other teacher's
        // account. Gated by [Authorize(Roles = "Teacher")] on the whole action
        // pair below — only someone who already holds a real Teacher login can
        // reach this, unlike the old public QuickLogin page. Students and
        // anonymous visitors get redirected to Login instead by the
        // [Authorize] filter before any of this code runs.
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Switch()
        {
            AppUser currentUser = await _userManager.GetUserAsync(User);

            List<Teacher> teachers = await _db.Teachers
                .Where(t => !t.IsDeleted && t.AppUserId != currentUser.Id)
                .OrderBy(t => t.Subject).ThenBy(t => t.LastName)
                .ToListAsync();

            return View(teachers);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<IActionResult> SwitchTo(int id)
        {
            Teacher? teacher = await _db.Teachers.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
            if (teacher == null) return NotFound();

            AppUser? targetUser = teacher.AppUserId != null
                ? await _userManager.FindByIdAsync(teacher.AppUserId)
                : null;

            if (targetUser == null)
            {
                string email = !string.IsNullOrWhiteSpace(teacher.Email)
                    ? teacher.Email
                    : $"teacher{teacher.Id}@lms.local";

                targetUser = await _userManager.FindByEmailAsync(email);

                if (targetUser == null)
                {
                    targetUser = new AppUser
                    {
                        UserName = email.Split('@')[0] + teacher.Id,
                        Email = email,
                        Name = teacher.FirstName,
                        Surname = teacher.LastName,
                        IsTeacher = true,
                        EmailConfirmed = true
                    };

                    string autoPassword = "Qa1!" + Guid.NewGuid().ToString("N").Substring(0, 10);
                    IdentityResult created = await _userManager.CreateAsync(targetUser, autoPassword);
                    if (!created.Succeeded) return RedirectToAction(nameof(Switch));
                }

                if (!await _userManager.IsInRoleAsync(targetUser, "Teacher"))
                {
                    await _userManager.AddToRoleAsync(targetUser, "Teacher");
                }

                teacher.AppUserId = targetUser.Id;
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(targetUser, isPersistent: false);
            return RedirectToAction("Index", "Chat");
        }
    }
}
