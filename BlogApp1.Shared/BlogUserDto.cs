using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BlogApp1.Shared
{
    public class BlogUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Twitter { get; set; }
        public string? LinkedIn { get; set; }
        public string? Github { get; set; }
        public string? Instagram { get; set; }
        public string? Medium { get; set; }
        public string[]? Roles { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public int? PostCount { get; set; }
        public int? ViewCount { get; set; }
        public bool? EmailNotifications { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public bool Theme { get; set; }
        public string? Language { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int[]? LikeId { get; set; }
        public int[]? SavedId { get; set; }
        public string[]? Following { get; set; }
        public string[]? Follower { get; set; }
        public int[]? ReadHistory { get; set; }
        public string[]? UserPreference { get; set; }

    }
    public class UpdateUserPreferenceRequest
    {
        public Guid UserId { get; set; }          // maps to blog_user.user_id
        public List<string> Preferences { get; set; } = new();
    }
}

