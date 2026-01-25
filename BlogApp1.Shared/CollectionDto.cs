    using Supabase.Postgrest.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace BlogApp1.Shared
    {
        // Represents a user's collection (folder of blogs)
        [Table("collections")]
        public class CollectionDto : Supabase.Postgrest.Models.BaseModel
        {
            [PrimaryKey("id")]
            public Guid Id { get; set; }

            [Column("user_id")]
            public Guid UserId { get; set; }

            [Column("collection_name")]
            public string CollectionName { get; set; } = "";
            [Column("description")]
            public string Description { get; set; } = "";

            [Column("blog_id")]
            public int[] BlogIds { get; set; } = Array.Empty<int>();

            [Column("created_at")]
            public DateTime CreatedAt { get; set; }
        }

    }
