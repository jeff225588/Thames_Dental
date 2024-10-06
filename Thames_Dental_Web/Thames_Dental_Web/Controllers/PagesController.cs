using Microsoft.AspNetCore.Mvc;

namespace Thames_Dental_Web.Controllers
{
    public class PagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Servicios()
        {
            return View();
        }

        public IActionResult Nosotros()
        {
            return View();
        }

        public IActionResult Equipo()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }
    }
}
