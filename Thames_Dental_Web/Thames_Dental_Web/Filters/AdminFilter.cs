using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Thames_Dental_Web.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        // Verifica si el rol del usuario es el adecuado antes de ejecutar la acción
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rolUsuario = filterContext.HttpContext.Session.GetString("NombreRol");

            if (rolUsuario != null && (rolUsuario == "Admin" || rolUsuario == "Empleado"))
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller","Autenticacion"},
                    { "action", "NotFound404" }
                });
            }
        }
    }
}
