using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class RagIngestionDto
    {
        public string BlogId { get; set; }
        public string Title { get; set; }
        public string HtmlContent { get; set; }
        public string AuthorName { get; set; }
        public string[] Tags { get; set; }
        public string Domain { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
