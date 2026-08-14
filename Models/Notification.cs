using LMS.Models.Base;

namespace LMS.Models
{
    // A lightweight in-app notification (course completed, certificate unlocked,
    // a student finished a teacher's course, etc). No email delivery — shown via
    // the bell icon in the navbar and the /Notification/Index page.
    public class Notification : BaseEntity
    {
        public string RecipientId { get; set; }
        public AppUser Recipient { get; set; }

        public string Title { get; set; } = "";
        public string Message { get; set; }
        public string Icon { get; set; } = "🔔";

        // Optional link the notification takes the user to when clicked
        // (e.g. the course page or the certificate page).
        public string? Url { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
