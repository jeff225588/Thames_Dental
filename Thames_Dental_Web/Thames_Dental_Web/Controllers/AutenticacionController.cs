using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class AutenticacionController : Controller
    {
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(UsuarioModel model)
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
