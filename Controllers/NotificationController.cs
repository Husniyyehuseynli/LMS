using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string filter = "all")
        {
            AppUser user = await _userManager.GetUserAsync(User);

            IQueryable<Notification> query = _db.Notifications
                .Where(n => n.RecipientId == user.Id && !n.IsDeleted);

            if (filter == "unread") query = query.Where(n => !n.IsRead);
            else if (filter == "read") query = query.Where(n => n.IsRead);

            List<Notification> notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            ViewBag.Filter = filter;
            ViewBag.UnreadCount = await _db.Notifications
                .CountAsync(n => n.RecipientId == user.Id && !n.IsDeleted && !n.IsRead);

            return View(notifications);
        }

      
        [HttpGet]
        public async Task<IActionResult> Unread()
        {
            AppUser user = await _userManager.GetUserAsync(User);

            var recent = await _db.Notifications
                .Where(n => n.RecipientId == user.Id && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Icon,
                    n.Url,
                    n.IsRead,
                    createdDate = n.CreatedDate.ToString("MMM d, HH:mm")
                })
                .ToListAsync();

            int unreadCount = await _db.Notifications.CountAsync(n => n.RecipientId == user.Id && !n.IsRead && !n.IsDeleted);

            return Json(new { unreadCount, recent });
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            Notification? notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == user.Id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            AppUser user = await _userManager.GetUserAsync(User);

            List<Notification> unread = await _db.Notifications
                .Where(n => n.RecipientId == user.Id && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var n in unread) n.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
