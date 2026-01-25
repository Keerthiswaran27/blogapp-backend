using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/chat-history")]
    public class ChatHistoryController : ControllerBase
    {
        private readonly Client _supabase;

        public ChatHistoryController(Client supabase)
        {
            _supabase = supabase;
        }

        // 1️⃣ INSERT CHAT MESSAGE
        [HttpPost("insert")]
        public async Task<IActionResult> Insert(ChatInsertDto dto)
        {
            var entity = new ChatHistory
            {
                AuthorUuid = dto.AuthorUuid,
                UserMessage = dto.UserMessage,
                AiMessage = dto.AiMessage,
                CreatedAt = DateTime.UtcNow
            };

            await _supabase
                .From<ChatHistory>()
                .Insert(entity);

            return Ok("Chat saved successfully");
        }

        // 2️⃣ FETCH CHAT HISTORY BY USER UUID
        [HttpGet("{authorUuid}")]
        public async Task<IActionResult> GetByUser(Guid authorUuid)
        {
            var result = await _supabase
                .From<ChatHistory>()
                .Where(x => x.AuthorUuid == authorUuid)
                .Order(x => x.CreatedAt,
                       Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var rows = result.Models;

            // GROUP BY DATE
            var grouped = rows
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new ChatHistoryGroupDto
                {
                    Date = g.Key,
                    Messages = g.Select(m => new ChatPairDto
                    {
                        Question = m.UserMessage,
                        Answer = m.AiMessage,
                        Time = m.CreatedAt
                    }).ToList()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return Ok(grouped);
        }
    }
}
