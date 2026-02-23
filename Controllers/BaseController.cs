using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpdeskSystem.Controllers
{
    /// <summary>
    /// BaseController enforces a simple session-based login check for derived controllers.
    /// It checks HttpContext.Session for a "UserId" value and redirects to /Account/Login when missing.
    /// This keeps authentication logic centralized and beginner-friendly without using Identity or middleware.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Called before an action method is executed.
        /// We override this to ensure the user is logged in (session contains UserId).
        /// If not logged in, we redirect to Account/Login.
        /// </summary>
        /// <param name="context">ActionExecutingContext provided by MVC.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Read the UserId from session. If it is not present, the user is not logged in.
            var userId = context.HttpContext.Session.GetInt32("UserId");

            // If UserId is null, set the result to a redirect to the login page.
            // This prevents the action from running and sends the user to /Account/Login.
            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return; // skip further processing
            }

            // If user is logged in, continue as normal.
            base.OnActionExecuting(context);
        }
    }
}