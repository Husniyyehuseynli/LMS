using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    [Area("Admin")]
    public class ContactMessageController : Controller
    {
        private readonly AppDbContext _db;

        public ContactMessageController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<ContactMessage> messages = await _db.ContactMessages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            ContactMessage? message = await _db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (message == null) return NotFound();

            message.IsRead = true;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            ContactMessage? message = await _db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (message == null) return NotFound();

            return View(message);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ContactMessage? message = await _db.ContactMessages.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (message == null) return NotFound();

            message.IsDeleted = true;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
