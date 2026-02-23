using System.Collections.Generic;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// View model for the Tickets index page which contains listing and filter values.
    /// </summary>
    public class TicketListViewModel
    {
        public List<TicketListItem> Tickets { get; set; } = new List<TicketListItem>();

        public string? Search { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }

        public int Page { get; set; }
        public int TotalPages { get; set; }

        // Categories used to populate filter dropdown.
        public List<Category> Categories { get; set; } = new List<Category>();
    }

    /// <summary>
    /// Lightweight ticket data for list display.
    /// </summary>
    public class TicketListItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public System.DateTime CreatedDate { get; set; }
    }
}
