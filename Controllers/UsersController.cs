using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    public class UsersController : BaseController
    {
        private readonly UserDb _userDb;

        public UsersController(UserDb userDb)
        {
            _userDb = userDb;
        }

        public IActionResult Index()
        {
            var users = _userDb.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User model)
        {
            // ensure server side validation passed before saving
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // prevent duplicate accounts with the same email
            if (_userDb.EmailExists(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                return View(model);
            }

            // enforce password strength rules
            if (!IsStrongPassword(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password must be at least 8 characters and include uppercase, lowercase, number and special character.");
                return View(model);
            }

            // store hashed password for security
            model.PasswordHash = UserDb.ComputeSha256Hash(model.Password);
            model.CreatedDate = DateTime.Now;
            _userDb.InsertUser(model);

            return RedirectToAction("Index");
        }

        // basic regex based password strength check
        private static bool IsStrongPassword(string? password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            var pattern = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}