using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Models;
using HelpdeskSystem.Data;
using System.Security.Cryptography;
using System.Text;

namespace HelpdeskSystem.Controllers
{
    /// <summary>
    /// AccountController handles simple login and logout using session only.
    /// No Identity or authentication middleware is used — this is intentionally simple for learning.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserDb _userDb;

        public AccountController(UserDb userDb)
        {
            _userDb = userDb;
        }

        /// <summary>
        /// GET /Account/Login
        /// Displays the login form and always provides a LoginViewModel to the view.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Always return a model instance so the Razor view has a strongly-typed model.
            return View(new LoginViewModel());
        }

        /// <summary>
        /// POST /Account/Login
        /// Validates input, checks user existence and password, sets session and redirects to Home/Index on success.
        /// Uses SHA256 hashing to compare the entered password (converted to HEX) against the stored PasswordHash.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            // Check model validation attributes first (Required, EmailAddress)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Fetch user by email from the database using UserDb.
            var user = _userDb.GetByEmail(model.Email);

            // If no user found, add model error and return view.
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // If user exists but not active, show error.
            if (!user.Value.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Account is inactive. Contact admin.");
                return View(model);
            }

            // Compute SHA256 hash of the entered password and compare with stored PasswordHash.
            // Database stores HEX string (e.g. "3EB3FE...") so we compute HEX in uppercase.
            var enteredHash = ComputeSha256Hash(model.Password);

            // Compare case-insensitive to be safe; both should represent the same HEX string.
            if (!string.Equals(enteredHash, user.Value.PasswordHash ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Authentication succeeded — store minimal user info in session.
            // Store UserId as an integer and FullName as a string.
            HttpContext.Session.SetInt32("UserId", user.Value.Id);
            HttpContext.Session.SetString("FullName", user.Value.FullName ?? string.Empty);

            // Redirect to Home/Index after successful login.
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Logout — clears the session and redirects to Login page.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear all session data.
            HttpContext.Session.Clear();

            // Redirect to the login page.
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Computes SHA256 hash of the provided input string and returns the HEX representation (uppercase).
        /// Uses System.Security.Cryptography.SHA256 and System.Text for encoding.
        /// </summary>
        /// <param name="input">Plain text password to hash.</param>
        /// <returns>HEX string in uppercase matching database format.</returns>
        private static string ComputeSha256Hash(string input)
        {
            // Guard against null to avoid exceptions.
            input ??= string.Empty;

            // Convert the input string to bytes using UTF8 encoding.
            var bytes = Encoding.UTF8.GetBytes(input);

            // Compute the SHA256 hash
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);

            // Convert hash bytes to HEX string (uppercase) to match DB format.
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2")); // X2 gives uppercase hex
            }

            return sb.ToString();
        }
    }
}
