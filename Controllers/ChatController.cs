using LMS.DAL;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly NotificationService _notifier;

        public ChatController(AppDbContext db, UserManager<AppUser> userManager, NotificationService notifier)
        {
            _db = db;
            _userManager = userManager;
            _notifier = notifier;
        }

  
        public async Task<IActionResult> Index()
        {
            AppUser user = await _userManager.GetUserAsync(User);
            bool isStudent = await _userManager.IsInRoleAsync(user, "Student");

            var partnerIds = await _db.ChatMessages
                .Where(m => m.SenderId == user.Id || m.ReceiverId == user.Id)
                .Select(m => m.SenderId == user.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var partners = await _db.Users.Where(u => partnerIds.Contains(u.Id)).ToListAsync();

            var conversations = partners.Select(p => new ChatConversationVM
            {
                OtherUserId = p.Id,
                OtherUserName = $"{p.Name} {p.Surname}",
                UnreadCount = _db.ChatMessages.Count(m => m.SenderId == p.Id && m.ReceiverId == user.Id && !m.IsRead),
                LastMessage = _db.ChatMessages
                    .Where(m => (m.SenderId == user.Id && m.ReceiverId == p.Id) || (m.SenderId == p.Id && m.ReceiverId == user.Id))
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault()
            }).ToList();

            ViewBag.Conversations = conversations;
            ViewBag.IsStudent = isStudent;

            if (isStudent)
            {
                ViewBag.Teachers = await _db.Teachers
                    .Where(t => !t.IsDeleted && t.AppUserId != null)
                    .OrderBy(t => t.Subject).ThenBy(t => t.LastName)
                    .ToListAsync();
            }

            return View();
        }

   
        public async Task<IActionResult> Conversation(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            AppUser user = await _userManager.GetUserAsync(User);
            AppUser? other = await _userManager.FindByIdAsync(id);
            if (other == null || other.Id == user.Id) return NotFound();

            var unread = await _db.ChatMessages
                .Where(m => m.SenderId == other.Id && m.ReceiverId == user.Id && !m.IsRead)
                .ToListAsync();
            foreach (var m in unread) m.IsRead = true;
            if (unread.Any()) await _db.SaveChangesAsync();

            var messages = await _db.ChatMessages
                .Where(m => (m.SenderId == user.Id && m.ReceiverId == other.Id) || (m.SenderId == other.Id && m.ReceiverId == user.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.OtherUserId = other.Id;
            ViewBag.OtherUserName = $"{other.Name} {other.Surname}";
            ViewBag.CurrentUserId = user.Id;

            Teacher? teacherProfile = await _db.Teachers.FirstOrDefaultAsync(t => t.AppUserId == other.Id);
            ViewBag.OtherUserSubject = teacherProfile?.Subject;

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Send(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrEmpty(receiverId))
            {
                return BadRequest();
            }

            AppUser user = await _userManager.GetUserAsync(User);
            AppUser? receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null || receiver.Id == user.Id) return BadRequest();

            ChatMessage message = new ChatMessage
            {
                SenderId = user.Id,
                ReceiverId = receiverId,
                Content = content.Trim()
            };
            await _db.ChatMessages.AddAsync(message);

           
            string preview = message.Content.Length > 80
                ? message.Content.Substring(0, 80) + "..."
                : message.Content;

            await _notifier.NotifyAsync(receiverId,
                $"Yeni mesaj — {user.Name} {user.Surname}",
                preview,
                $"/Chat/Conversation/{user.Id}",
                "💬");

            await _db.SaveChangesAsync();

            return Json(new
            {
                id = message.Id,
                senderId = message.SenderId,
                content = message.Content,
                sentAt = message.SentAt.ToString("HH:mm")
            });
        }


        [HttpGet]
        public async Task<IActionResult> Poll(string otherUserId, int afterId = 0)
        {
            AppUser user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(otherUserId)) return BadRequest();

            var newMessages = await _db.ChatMessages
                .Where(m => m.Id > afterId &&
                    ((m.SenderId == user.Id && m.ReceiverId == otherUserId) ||
                     (m.SenderId == otherUserId && m.ReceiverId == user.Id)))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var toMarkRead = newMessages.Where(m => m.ReceiverId == user.Id && !m.IsRead).ToList();
            foreach (var m in toMarkRead) m.IsRead = true;
            if (toMarkRead.Any()) await _db.SaveChangesAsync();

            return Json(newMessages.Select(m => new
            {
                id = m.Id,
                senderId = m.SenderId,
                content = m.Content,
                sentAt = m.SentAt.ToString("HH:mm")
            }));
        }
    }

    public class ChatConversationVM
    {
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessage { get; set; }
    }
}
