using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;

namespace BlogApp1.Shared
{
    [Table("blog_user")]
    public class BlogUser : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("bio")]
        public string? Bio { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("website_url")]
        public string? WebsiteUrl { get; set; }

        [Column("twitter")]
        public string? Twitter { get; set; }

        [Column("linkedin")]
        public string? LinkedIn { get; set; }

        [Column("github")]
        public string? Github { get; set; }

        [Column("instagram")]
        public string? Instagram { get; set; }

        [Column("medium")]
        public string? Medium { get; set; }

        [Column("roles")]
        public string[] Roles { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_verified")]
        public bool IsVerified { get; set; } = false;

        [Column("post_count")]
        public int? PostCount { get; set; } = 0;

        [Column("view_count")]
        public int? ViewCount { get; set; } = 0;

        [Column("email_notifications")]
        public bool? EmailNotifications { get; set; } = true;

        [Column("newsletter_subscribed")]
        public bool NewsletterSubscribed { get; set; } = false;

        [Column("theme")]
        public bool Theme { get; set; } = false;

        [Column("language")]
        public string? Language { get; set; } = "en";

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [Column("liked_id")]
        public int[]? LikeId { get; set; }
        [Column("saved_id")]
        public int[] SavedId  { get; set; }
        [Column("following")]
        public string[]? Following { get; set; } 
        [Column("follower")]
        public string[]? Follower { get; set; }
        [Column("read_history")]
        public int[]? ReadHistory { get; set; } 
        [Column("user_preference")]
        public string[]? UserPreference { get; set; } 

    }
    public class UserStatsDto
    {
        public Guid UserId { get; set; }

        public int LikedCount { get; set; }

        public int SavedCount { get; set; }

        public int ReadHistoryCount { get; set; }

        public int CommentCount { get; set; }
    }
}
