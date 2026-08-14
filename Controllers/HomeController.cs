using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly NotificationService _notifier;

        public HomeController(AppDbContext db, UserManager<AppUser> userManager, NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _notifier = notifier;
        }

        // Sends an in-app bell notification to every Admin/Instructor account —
        // the same roles that can access the ContactMessage and
        // TeacherApplication admin pages. Used for things a real person needs
        // to act on soon that otherwise sit silently in a table until someone
        // happens to open that admin page.
        private async Task NotifyAdminsAsync(string title, string message, string url, string icon)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");

            var recipients = admins.Concat(instructors).GroupBy(u => u.Id).Select(g => g.First());
            foreach (var recipient in recipients)
            {
                await _notifier.NotifyAsync(recipient.Id, title, message, url, icon);
            }
        }

        public async Task<IActionResult> Index()
        {
            List<Course> featuredCourses = await _db.Courses
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Id)
                .Take(6)
                .ToListAsync();

            List<Category> categories = await _db.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.Courses)
                .ToListAsync();

            ViewBag.Categories = categories;

            // Homepage stat counters (animated in JS)
            ViewBag.StudentCount = await _db.Enrollments.Select(e => e.StudentId).Distinct().CountAsync();
            ViewBag.CourseCount = await _db.Courses.CountAsync(c => !c.IsDeleted);
            ViewBag.TeacherCount = await _db.Teachers.CountAsync(t => !t.IsDeleted);

            return View(featuredCourses);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ContactError"] = "Please fill in all fields.";
                return RedirectToAction("Contact");
            }

            var contactMessage = new ContactMessage
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Subject = subject.Trim(),
                Message = message.Trim(),
                SentDate = DateTime.Now
            };

            _db.ContactMessages.Add(contactMessage);
            await _db.SaveChangesAsync();

            await NotifyAdminsAsync(
                "Yeni əlaqə mesajı",
                $"{contactMessage.Name}: {contactMessage.Subject}",
                "/Admin/ContactMessage",
                "✉️");
            await _db.SaveChangesAsync();

            TempData["ContactSuccess"] = "Thanks! Your message has been sent — we'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult BecomeTeacher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeTeacher(string firstName, string lastName, string email, string subject, string bio)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(bio))
            {
                TempData["ApplyError"] = "Please fill in all fields.";
                return RedirectToAction("BecomeTeacher");
            }

            bool alreadyPending = await _db.TeacherApplications
                .AnyAsync(a => a.Email == email.Trim() && a.Status == ApplicationStatus.Pending && !a.IsDeleted);
            if (alreadyPending)
            {
                TempData["ApplyError"] = "You already have a pending application with this email.";
                return RedirectToAction("BecomeTeacher");
            }

            var application = new TeacherApplication
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim(),
                Subject = subject.Trim(),
                Bio = bio.Trim(),
                AppliedDate = DateTime.Now
            };

            _db.TeacherApplications.Add(application);
            await _db.SaveChangesAsync();

            await NotifyAdminsAsync(
                "Yeni müəllim müraciəti",
                $"{application.FirstName} {application.LastName} — {application.Subject}",
                "/Admin/TeacherApplication",
                "🎓");
            await _db.SaveChangesAsync();

            TempData["ApplySuccess"] = "Thanks! Your application has been submitted — our team will review it and get back to you.";
            return RedirectToAction("BecomeTeacher");
        }
    }
}
