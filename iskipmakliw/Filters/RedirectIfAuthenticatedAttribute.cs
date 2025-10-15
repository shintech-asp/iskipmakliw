using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace iskipmakliw.Filters
{
    public class RedirectIfAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var user = context.HttpContext.User;

                if (user.IsInRole("Admin"))
                    context.Result = new RedirectToActionResult("Index", "Admin", null);
                else if (user.IsInRole("Seller"))
                    context.Result = new RedirectToActionResult("Index", "Seller", null);
                else if (user.IsInRole("Customer"))
                    context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            base.OnActionExecuting(context);
        }
    }
}