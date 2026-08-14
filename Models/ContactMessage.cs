using LMS.Models.Base;

namespace LMS.Models
{
    public class ContactMessage : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
