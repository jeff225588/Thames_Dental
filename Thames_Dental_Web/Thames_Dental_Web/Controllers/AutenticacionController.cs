using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class AutenticacionController : Controller
    {
        public IActionResult Registrar()
        {
            return View();
        }

        public IActionResult RegistrarAdmin()
        {
            return View();
        }

        public IActionResult Ingresar()
        {
            return View();
        }

        public IActionResult RecuperarContrasena()
        {
            return View();
        }
    }
}
