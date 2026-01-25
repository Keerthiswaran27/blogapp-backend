using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class RagQueryRequest
    {
        public string Query { get; set; }
        public int TopK { get; set; } = 5;
    }
    public class RagQueryResponse
    {
        public string Text { get; set; }
        public float Score { get; set; }
    }
    public class RagChunkResult
    {
        public string Text { get; set; }
        public float Score { get; set; }
    }
    public class BotReply
    {
        public string Answer { get; set; }
        public List<SourceSnippet> Sources { get; set; }
    }

    public class SourceSnippet
    {
        public string Content { get; set; }
        public float Similarity { get; set; }
    }


}
