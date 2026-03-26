using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CSI402_Project.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole) || (userRole != "Admin" && userRole != "SuperAdmin"))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }

    public class SuperAdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole) || userRole != "SuperAdmin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
