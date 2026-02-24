using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly CategoryDb _categoryDb;

        public CategoriesController(CategoryDb categoryDb)
        {
            _categoryDb = categoryDb;
        }

        public IActionResult Index()
        {
            var list = _categoryDb.GetAllCategories();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_categoryDb.NameExists(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                return View(model);
            }

            model.CreatedDate = DateTime.Now;
            _categoryDb.InsertCategory(model);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id)
        {
            _categoryDb.ToggleIsActive(id);
            return RedirectToAction("Index");
        }
    }
}