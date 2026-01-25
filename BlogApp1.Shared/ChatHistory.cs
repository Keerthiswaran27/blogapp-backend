using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace BlogApp1.Shared
{
    [Table("chat_history")]
    public class ChatHistory : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("author_uuid")]
        public Guid AuthorUuid { get; set; }

        [Column("user_message")]
        public string UserMessage { get; set; }

        [Column("ai_message")]
        public string AiMessage { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
    public class ChatHistoryDto
    {
        public Guid Id { get; set; }
        public Guid AuthorUuid { get; set; }

        public string UserMessage { get; set; }
        public string AiMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
    public class ChatInsertDto
    {
        public Guid AuthorUuid { get; set; }
        public string UserMessage { get; set; }
        public string AiMessage { get; set; }
    }
    public class ChatHistoryGroupDto
    {
        public DateTime Date { get; set; }
        public List<ChatPairDto> Messages { get; set; }
    }

    public class ChatPairDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime Time { get; set; }
    }

}
