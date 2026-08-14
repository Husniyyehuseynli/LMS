using LMS.Models.Base;

namespace LMS.Models
{
    // A simple direct message between two platform accounts (student <-> teacher).
    public class ChatMessage : BaseEntity
    {
        public string SenderId { get; set; }
        public AppUser Sender { get; set; }

        public string ReceiverId { get; set; }
        public AppUser Receiver { get; set; }

        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}
