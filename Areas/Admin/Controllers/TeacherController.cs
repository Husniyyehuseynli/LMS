using LMS.Areas.Admin.ViewModels.Teacher;
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
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public TeacherController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Teacher> teachers = await _db.Teachers
                .Include(t => t.Courses)
                .OrderBy(t => t.Subject)
                .ToListAsync();
            return View(teachers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeacherVM teacherVM)
        {
            if (teacherVM.PhotoFile != null)
            {
                if (!teacherVM.PhotoFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError(nameof(teacherVM.PhotoFile), "File must be an image");
                    return View(teacherVM);
                }
                if (teacherVM.PhotoFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(teacherVM.PhotoFile), "File size can not exceed 2MB");
                    return View(teacherVM);
                }
            }
            if (!ModelState.IsValid) return View(teacherVM);

            LMS.Models.Teacher teacher = new LMS.Models.Teacher()
            {
                FirstName = teacherVM.FirstName,
                LastName = teacherVM.LastName,
                Subject = teacherVM.Subject,
                Bio = teacherVM.Bio,
                Email = teacherVM.Email,
                PhotoUrl = teacherVM.PhotoFile?.SaveImage(_env, "uploads/teachers")
            };

            await _db.Teachers.AddAsync(teacher);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            LMS.Models.Teacher? teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();
            teacher.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int? id)
        {
            LMS.Models.Teacher? teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();
            teacher.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            LMS.Models.Teacher? teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            UpdateTeacherVM teacherVM = new UpdateTeacherVM()
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Subject = teacher.Subject,
                Bio = teacher.Bio,
                Email = teacher.Email,
                PhotoUrl = teacher.PhotoUrl
            };
            return View(teacherVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateTeacherVM teacherVM)
        {
            if (teacherVM.PhotoFile != null)
            {
                if (!teacherVM.PhotoFile.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError(nameof(teacherVM.PhotoFile), "File must be an image");
                    return View(teacherVM);
                }
                if (teacherVM.PhotoFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(teacherVM.PhotoFile), "File size can not exceed 2MB");
                    return View(teacherVM);
                }
            }
            if (!ModelState.IsValid) return View(teacherVM);

            LMS.Models.Teacher? oldTeacher = await _db.Teachers.FindAsync(teacherVM.Id);
            if (oldTeacher == null) return NotFound();

            oldTeacher.FirstName = teacherVM.FirstName;
            oldTeacher.LastName = teacherVM.LastName;
            oldTeacher.Subject = teacherVM.Subject;
            oldTeacher.Bio = teacherVM.Bio;
            oldTeacher.Email = teacherVM.Email;

            if (teacherVM.PhotoFile != null)
            {
                oldTeacher.PhotoUrl = teacherVM.PhotoFile.SaveImage(_env, "uploads/teachers");
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
