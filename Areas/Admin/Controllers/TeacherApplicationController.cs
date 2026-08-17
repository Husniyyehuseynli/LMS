using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class TeacherApplicationController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public TeacherApplicationController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            List<TeacherApplication> applications = await _db.TeacherApplications
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();

            return View(applications);
        }

   
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            TeacherApplication? application = await _db.TeacherApplications
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            if (application == null) return NotFound();

            AppUser? user = await _userManager.FindByEmailAsync(application.Email);
            bool createdNewAccount = false;
            string? generatedPassword = null;

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = application.Email.Split('@')[0],
                    Email = application.Email,
                    Name = application.FirstName,
                    Surname = application.LastName,
                    IsTeacher = true,
                    EmailConfirmed = true
                };

                generatedPassword = "Qa1!" + Guid.NewGuid().ToString("N").Substring(0, 10);
                IdentityResult created = await _userManager.CreateAsync(user, generatedPassword);
                if (!created.Succeeded)
                {
                    TempData["ApplicationError"] = "Could not create a login account for this email — it may already be in use differently.";
                    return RedirectToAction("Index");
                }
                createdNewAccount = true;
            }

            if (!await _userManager.IsInRoleAsync(user, "Teacher"))
            {
                await _userManager.AddToRoleAsync(user, "Teacher");
            }

            var teacher = new Teacher
            {
                FirstName = application.FirstName,
                LastName = application.LastName,
                Subject = application.Subject,
                Bio = application.Bio,
                Email = application.Email,
                AppUserId = user.Id
            };

            _db.Teachers.Add(teacher);
            application.Status = ApplicationStatus.Approved;
            await _db.SaveChangesAsync();

      
            TempData["ApplicationSuccess"] = createdNewAccount
                ? $"{application.FirstName} {application.LastName} is now a teacher. Login: {application.Email} — Temporary password: {generatedPassword} (share this with them — it is shown only once and cannot be recovered afterwards; ask them to change it after first login)."
                : $"{application.FirstName} {application.LastName} is now a teacher. They already had a login ({application.Email}) — let them know their existing password now also gives Teacher access.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            TeacherApplication? application = await _db.TeacherApplications
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            if (application == null) return NotFound();

            application.Status = ApplicationStatus.Rejected;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
