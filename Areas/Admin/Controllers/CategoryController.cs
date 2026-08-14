using LMS.Areas.Admin.ViewModels.Category;
using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories
                .Include(c => c.Courses)
                .ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid) return View(categoryVM);

            Category category = new Category()
            {
                Name = categoryVM.Name,
                Icon = categoryVM.Icon,
            };
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Category? category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();
            category.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int? id)
        {
            Category? category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();
            category.IsDeleted = false;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            Category? category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            UpdateCategoryVM categoryVM = new UpdateCategoryVM()
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
            };
            return View(categoryVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCategoryVM categoryVM)
        {
            if (!ModelState.IsValid) return View(categoryVM);

            Category? oldCategory = await _db.Categories.FindAsync(categoryVM.Id);
            if (oldCategory == null) return NotFound();

            oldCategory.Name = categoryVM.Name;
            oldCategory.Icon = categoryVM.Icon;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
