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



        [HttpGet]
        public IActionResult RegistrarAdmin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RegistrarAdmin(UsuarioModel model)
        {
            return View();
        }



        [HttpGet]
        public IActionResult Ingresar()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Ingresar(UsuarioModel model)
        {
            return View();
        }



        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RecuperarContrasena(UsuarioModel model)
        {
            return View();
        }
    }
}
