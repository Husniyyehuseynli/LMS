using LMS.Models;

namespace LMS.DAL
{
    // Thin helper around AppDbContext for creating notifications, so
    // controllers don't repeat the same four lines everywhere. Callers still
    // own SaveChangesAsync() — this only queues the entity.
    public class NotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(string recipientId, string title, string message, string? url = null, string icon = "🔔")
        {
            if (string.IsNullOrEmpty(recipientId)) return;

            await _db.Notifications.AddAsync(new Notification
            {
                RecipientId = recipientId,
                Title = title,
                Message = message,
                Icon = icon,
                Url = url,
                IsRead = false,
                CreatedDate = DateTime.Now
            });
        }
    }
}
