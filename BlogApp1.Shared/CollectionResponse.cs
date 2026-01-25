using Supabase.Postgrest.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class CollectionResponse
    {
       
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CollectionName { get; set; } = "";
        public string Description { get; set; } = "";
        public int[] BlogIds { get; set; } = Array.Empty<int>();
        public DateTime CreatedAt { get; set; }
    }
}
