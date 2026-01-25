using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace BlogApp1.Shared
{
    [Table("users")] // points to auth.users
    public class AuthUser : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        public string Aud { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public DateTime? EmailConfirmedAt { get; set; }
        public string Phone { get; set; }
        public DateTime? PhoneConfirmedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string RawUserMetaData { get; set; } // JSON as string
        public bool IsSuperAdmin { get; set; }
    }
}
