using System;
using System.Collections.Generic;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// View model used for ticket details page including comments and add-comment form.
    /// </summary>
    public class TicketDetailsViewModel
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Comments currently associated with the ticket.
        public List<CommentItem> Comments { get; set; } = new List<CommentItem>();

        // Bound to the add-comment form.
        public string NewCommentText { get; set; } = string.Empty;

        /// <summary>
        /// Lightweight comment data for display.
        /// </summary>
        public class CommentItem
        {
            public int Id { get; set; }
            public string CommentText { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
            public string CreatedByName { get; set; } = string.Empty;
        }
    }
}
