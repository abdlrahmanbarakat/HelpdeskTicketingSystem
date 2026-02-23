using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    /// <summary>
    /// Controller for managing categories. Inherits BaseController to require login.
    /// Only List and Create actions are implemented for simplicity.
    /// </summary>
    public class CategoriesController : BaseController
    {
        private readonly CategoryDb _categoryDb;

        public CategoriesController(CategoryDb categoryDb)
        {
            _categoryDb = categoryDb;
        }

        /// <summary>
        /// GET: /Categories
        /// Shows a list of categories.
        /// </summary>
        public IActionResult Index()
        {
            var list = _categoryDb.GetAllCategories();
            return View(list);
        }

        /// <summary>
        /// GET: /Categories/Create
        /// Shows a simple create form.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        /// <summary>
        /// POST: /Categories/Create
        /// Validates model, ensures name is unique, sets CreatedDate and inserts.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check unique name (case-insensitive by DB collation may vary; keep simple)
            if (_categoryDb.NameExists(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            _categoryDb.InsertCategory(model);

            return RedirectToAction("Index");
        }
    }
}
