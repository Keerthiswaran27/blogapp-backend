using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared.EditorModels
{
    public class RevisionModel
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int VersionNo { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? EditorUid { get; set; }
        public string? AuthorUid { get; set; }
        public bool IsCurrent { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }

}
