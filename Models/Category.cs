using System;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// Category model for ticket categorization.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Primary key from the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Category name. Required for display and creation.
        /// </summary>
        [Required(ErrorMessage = "Category name is required.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Whether the category is active and available for use.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the record was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
