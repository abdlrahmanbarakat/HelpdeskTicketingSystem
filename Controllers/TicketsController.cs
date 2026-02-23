using Microsoft.AspNetCore.Mvc;
using HelpdeskSystem.Data;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Controllers
{
    /// <summary>
    /// Controller for creating and listing tickets. Inherits BaseController to require login.
    /// </summary>
    public class TicketsController : BaseController
    {
        private readonly TicketDb _ticketDb;

        public TicketsController(TicketDb ticketDb)
        {
            _ticketDb = ticketDb;
        }

        /// <summary>
        /// GET: /Tickets
        /// Shows paged list of tickets with optional filters.
        /// </summary>
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

        /// <summary>
        /// GET: /Tickets/Create
        /// Shows a create form with active categories for dropdown.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _ticketDb.GetActiveCategoriesForDropdown();
            return View(new Ticket());
        }

        /// <summary>
        /// POST: /Tickets/Create
        /// Validates input, sets CreatedBy from session, sets defaults and inserts.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _ticketDb.GetActiveCategoriesForDropdown();
                return View(model);
            }

            // Get current user id from session (BaseController ensures user is logged in)
            var userId = HttpContext.Session.GetInt32("UserId");
            model.CreatedBy = userId ?? 0;
            model.Status = "Open";
            model.CreatedDate = DateTime.Now;
            model.IsDeleted = false;

            _ticketDb.InsertTicket(model);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// GET: /Tickets/Details/{id}
        /// Shows ticket details and comments.
        /// </summary>
        [HttpGet]
        public IActionResult Details(int id)
        {
            var ticket = _ticketDb.GetTicketDetails(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Comments = _ticketDb.GetCommentsForTicket(id);
            return View(ticket);
        }

        /// <summary>
        /// POST: /Tickets/AddComment/{id}
        /// Adds a new comment to the ticket unless the ticket is closed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(int id, TicketDetailsViewModel model)
        {
            // Server-side validation: new comment must not be empty.
            if (string.IsNullOrWhiteSpace(model.NewCommentText))
            {
                ModelState.AddModelError(nameof(model.NewCommentText), "Comment cannot be empty.");
            }

            // Retrieve ticket details to re-render view if needed.
            var ticket = _ticketDb.GetTicketDetails(id);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ticket.Comments = _ticketDb.GetCommentsForTicket(id);
                // Preserve the attempted new comment text so user doesn't lose it.
                ticket.NewCommentText = model.NewCommentText;
                return View("Details", ticket);
            }

            // Get current user id from session (BaseController ensures login)
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

        /// <summary>
        /// POST: /Tickets/Delete
        /// Soft-deletes a ticket by setting IsDeleted = 1. Redirects back to index.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _ticketDb.SoftDeleteTicket(id);
            return RedirectToAction("Index");
        }
    }
}
