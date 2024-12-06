using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Thames_Dental_Web.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        // Verifica si el rol del usuario es el adecuado antes de ejecutar la acción
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var rolUsuario = filterContext.HttpContext.Session.GetString("RolID");

            // Compara si el rol del usuario es "1" (el rol admin en este caso)
            if (rolUsuario != null && rolUsuario == "3")
            {
                base.OnActionExecuting(filterContext); // Procede con la acción si es admin
            }
            else
            {
                // Redirige a la página principal si el rol no es "1"
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller","Pages"},
                    { "action", "Index" }
                });
            }
        }
    }
}
