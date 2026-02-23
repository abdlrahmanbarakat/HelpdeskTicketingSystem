using System;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// Ticket model used for creating tickets and mapping to DB.
    /// </summary>
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        // Id of the user who created the ticket. Filled from Session when creating.
        public int CreatedBy { get; set; }

        // Simple status text (e.g. Open, InProgress, Closed)
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        // Soft delete flag.
        public bool IsDeleted { get; set; }
    }
}
