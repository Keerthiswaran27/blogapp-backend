using System;

namespace BlogApp1.Shared.EditorModels
{
    public class BlogSummaryModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Domain { get; set; } = "General";
        public string Status { get; set; } = "pending";
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? EditorUid { get; set; }

        // 👇 Added field for Rejected tab display
        public string? RejectionReason { get; set; }

        // (Optional) If you ever want to show approval notes
        public string? ReviewComments { get; set; }
    }
}
