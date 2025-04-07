using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Chat/InsertChat
        [HttpPost("InsertChat")]
        public async Task<IActionResult> InsertChat([FromBody] Chat chat)
        {
            // استخراج اسم المستخدم الذي أرسل الرسالة
            chat.FromUserName = await _context.Users
                .Where(x => x.Id == chat.FromUserId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            // إضافة الشات إلى قاعدة البيانات
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // معالجة حالة المستخدمين في الغرفة
            List<RoomUser> roomUsers = await _context.RoomUsers
                .Where(x => x.RoomId == chat.RoomId)
                .ToListAsync();

            var chatStatues = new List<ChatStatue>();

            foreach (var user in roomUsers)
            {
                if (user.Id != chat.FromUserId)
                {
                    var chatStatue = new ChatStatue
                    {
                        ChatId = chat.Id,
                        RoomId = chat.RoomId,
                        UserId = user.Id,
                        UserName = await _context.Users.Where(x => x.Id == user.Id).Select(x => x.Name).FirstOrDefaultAsync(),
                        MessageId = chat.Id,
                        IsDelivered = false,
                        IsRead = false
                    };
                    chatStatues.Add(chatStatue);
                }
            }

            _context.ChatStatues.AddRange(chatStatues);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Chat/UpdateIsDelivered/{userId}
        [HttpPut("UpdateIsDelivered/{userId}")]
        public async Task<IActionResult> UpdateIsDelivered(int userId)
        {
            var chatStatue = await _context.ChatStatues
                .Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();

            if (chatStatue == null)
                return NotFound();

            chatStatue.IsDelivered = true;
            _context.Entry(chatStatue).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Chat/UpdateIsRead/{userId}/{roomId}
        [HttpPut("UpdateIsRead/{userId}/{roomId}")]
        public async Task<IActionResult> UpdateIsRead(int userId, int roomId)
        {
            var chatStatue = await _context.ChatStatues
                .Where(x => x.UserId == userId && x.RoomId == roomId)
                .FirstOrDefaultAsync();

            if (chatStatue == null)
                return NotFound();

            chatStatue.IsRead = true;
            _context.Entry(chatStatue).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/Chat/GetChat/{roomId}
        [HttpGet("GetChat/{roomId}")]
        public async Task<ActionResult<IEnumerable<Chat>>> GetChat(int roomId)
        {
            var chats = await _context.Chats
                .Where(x => x.RoomId == roomId)
                .Include(x => x.ChatStatues)
                .ToListAsync();

            return Ok(chats);
        }

        // GET: api/Chat/GetMessageStatue/{chatId}
        [HttpGet("GetMessageStatue/{chatId}")]
        public async Task<ActionResult<IEnumerable<ChatStatue>>> GetMessageStatue(int chatId)
        {
            var chatStatue = await _context.ChatStatues
                .Where(x => x.ChatId == chatId)
                .ToListAsync();

            return Ok(chatStatue);
        }

        // GET: api/Chat/GetUnDeliveredChat/{userId}
        [HttpGet("GetUnDeliveredChat/{userId}")]
        public async Task<ActionResult<IEnumerable<ChatStatue>>> GetUnDeliveredChat(int userId)
        {
            var chatStatue = await _context.ChatStatues
                .Include(x => x.Chat)
                .Where(x => x.UserId == userId && x.IsDelivered == false)
                .ToListAsync();

            return Ok(chatStatue);
        }
    }
}
