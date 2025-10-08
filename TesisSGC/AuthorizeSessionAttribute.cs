using System.Web.Mvc;

namespace TesisSGC
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            // Ignorar Login y Logout
            if (controller == "Login" && (action == "Login" || action == "Logout"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Si no hay sesión, redirige al Login
            if (session["UsuarioID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Login" }
                    });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
