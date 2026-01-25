using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared.EditorModels
{
    public class DashboardStatsModel
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int RevisionCount { get; set; }
    }
    public class EditorProfileResponse
    {
        public BlogUserDto? User { get; set; }
        public List<BlogDto> ReviewedBlogs { get; set; } = new();
    }
}
