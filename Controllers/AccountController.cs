using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Models;
using HelpdeskSystem.Data;
using System.Security.Cryptography;
using System.Text;

namespace HelpdeskSystem.Controllers
{
    // Simple account controller using session-based authentication
    public class AccountController : Controller
    {
        private readonly UserDb _userDb;

        public AccountController(UserDb userDb)
        {
            _userDb = userDb;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userDb.GetByEmail(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            if (!user.Value.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Account is inactive. Contact admin.");
                return View(model);
            }

            var enteredHash = ComputeSha256Hash(model.Password);
            if (!string.Equals(enteredHash, user.Value.PasswordHash ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Value.Id);
            HttpContext.Session.SetString("FullName", user.Value.FullName ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // Compute SHA256 and return uppercase HEX string
        private static string ComputeSha256Hash(string input)
        {
            input ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}