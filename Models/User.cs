using System;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// User model used for displaying users and creating new users.
    /// This is a simple model kept beginner-friendly.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key from the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of the user. Required for display and registration.
        /// </summary>
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user. Required and must be a valid email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain text password provided when creating a user. Marked required for create form.
        /// (Not stored in database; used to compute PasswordHash before inserting.)
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// SHA256 hex password hash stored in the database.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the record was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}