using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared.EditorModels
{
    public class AnalyticsModel
    {
        public int TotalReviewed { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public int TotalPending { get; set; }
        public double AverageReviewTimeHours { get; set; }
        public List<string> TopAuthors { get; set; } = new();
        public Dictionary<string, int> MonthlyApprovals { get; set; } = new();
    }

}
