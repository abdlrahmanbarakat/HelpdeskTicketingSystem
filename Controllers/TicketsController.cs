using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly TicketDb _ticketDb;

        public TicketsController(TicketDb ticketDb)
        {
            _ticketDb = ticketDb;
        }

        public IActionResult Index(string? search, string? status, int? categoryId, int page = 1)
        {
            const int pageSize = 10;
            var totalCount = _ticketDb.GetTicketsCount(search, status, categoryId);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var tickets = _ticketDb.GetTicketsPaged(search, status, categoryId, page, pageSize);

            var vm = new TicketListViewModel
            {
                Tickets = tickets,
                Search = search,
                Status = status,
                CategoryId = categoryId,
                Page = page,
                TotalPages = totalPages,
                Categories = _ticketDb.GetAllCategoriesForFilter()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateCategories();
            return View(new Ticket());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket model)
        {
            // verify model state to avoid saving invalid data
            if (!ModelState.IsValid)
            {
                PopulateCategories();
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            model.CreatedBy = userId ?? 0;
            model.Status = "Open";
            model.CreatedDate = DateTime.Now;
            model.IsDeleted = false;

            _ticketDb.InsertTicket(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var ticket = _ticketDb.GetTicketDetails(id);
            if (ticket == null) return NotFound();
            ticket.Comments = _ticketDb.GetCommentsForTicket(id);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int id, TicketDetailsViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewCommentText))
            {
                ModelState.AddModelError(nameof(model.NewCommentText), "Comment cannot be empty.");
            }

            var ticket = _ticketDb.GetTicketDetails(id);
            if (ticket == null) return NotFound();

            // check model validity before attempting to add a comment
            if (!ModelState.IsValid)
            {
                ticket.Comments = _ticketDb.GetCommentsForTicket(id);
                ticket.NewCommentText = model.NewCommentText;
                return View("Details", ticket);
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var added = _ticketDb.AddComment(id, model.NewCommentText, userId);
            if (!added)
            {
                ModelState.AddModelError(string.Empty, "Cannot add comments to a closed ticket.");
                ticket.Comments = _ticketDb.GetCommentsForTicket(id);
                ticket.NewCommentText = model.NewCommentText;
                return View("Details", ticket);
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // mark ticket as deleted to retain history and avoid cascade issues
            _ticketDb.SoftDeleteTicket(id);
            return RedirectToAction("Index");
        }

        private void PopulateCategories()
        {
            ViewBag.Categories = _ticketDb.GetActiveCategoriesForDropdown();
        }
    }
}