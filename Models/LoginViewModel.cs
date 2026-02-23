using System.ComponentModel.DataAnnotations;

namespace HelpdeskSystem.Models
{
    /// <summary>
    /// ViewModel used to capture user credentials from the login form.
    /// This class is simple and intended for model-binding in Razor Pages or MVC.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// The user's email address submitted from the login form.
        /// - [Required] enforces that a value must be provided.
        /// - [EmailAddress] validates the value is in a correct email format.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's password submitted from the login form.
        /// - [Required] enforces that a value must be provided.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
