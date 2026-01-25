using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    [Table("newsletter_subscription")]
    public class NewsletterSubscription : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("email_id")]
        public string EmailId { get; set; } = string.Empty;

        [Column("user_uuid")]
        public string UserUuid { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
    public class NewsletterSubscriptionDto
    {
        public long Id { get; set; }

        public string EmailId { get; set; } = string.Empty;

        public string UserUuid { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
    public class CreateNewsletterSubscriptionRequest
    {
        public string EmailId { get; set; } = string.Empty;
        public string UserUuid { get; set; } = string.Empty;
    }

    public class CheckNewsletterSubscriptionRequest
    {
        public string UserUuid { get; set; } = string.Empty;
    }

    public class CheckNewsletterSubscriptionResponse
    {
        public string Status { get; set; } = "not_present"; // "present" or "not_present"
    }
}
