using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    /// <summary>
    /// UsersController manages basic user creation and listing.
    /// Inherits from BaseController so actions require a logged-in session.
    /// Uses ADO.NET via UserDb for database operations.
    /// </summary>
    public class UsersController : BaseController
    {
        private readonly UserDb _userDb;

        public UsersController(UserDb userDb)
        {
            _userDb = userDb;
        }

        /// <summary>
        /// GET: /Users
        /// Shows a simple list of users.
        /// </summary>
        public IActionResult Index()
        {
            var users = _userDb.GetAllUsers();
            return View(users);
        }

        /// <summary>
        /// GET: /Users/Create
        /// Displays the create user form.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        /// <summary>
        /// POST: /Users/Create
        /// Validates model, ensures unique email, enforces a simple strong-password rule,
        /// hashes the password using SHA256 HEX via UserDb helper, sets CreatedDate and inserts.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User model)
        {
            // Check model validation attributes first (Required fields)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for unique email using UserDb
            if (_userDb.EmailExists(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                return View(model);
            }

            // Strong password enforcement: min 8 chars, uppercase, lowercase, number and special char
            if (!IsStrongPassword(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password must be at least 8 characters and include uppercase, lowercase, number and special character.");
                return View(model);
            }

            // Compute SHA256 HEX password hash using helper from UserDb
            model.PasswordHash = UserDb.ComputeSha256Hash(model.Password);

            // Set creation date and default active flag if needed
            model.CreatedDate = DateTime.Now;

            // Insert user into database (InsertUser expects PasswordHash populated)
            _userDb.InsertUser(model);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Simple regex-based password strength checker.
        /// - At least 8 characters
        /// - At least one uppercase letter
        /// - At least one lowercase letter
        /// - At least one digit
        /// - At least one special character
        /// This kept as an internal helper to remain beginner-friendly.
        /// </summary>
        private static bool IsStrongPassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // Regex explanation:
            // (?=.*[a-z])    -> at least one lowercase
            // (?=.*[A-Z])    -> at least one uppercase
            // (?=.*\d)      -> at least one digit
            // (?=.*[^A-Za-z0-9]) -> at least one special char
            // .{8,}          -> at least 8 characters
            var pattern = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
