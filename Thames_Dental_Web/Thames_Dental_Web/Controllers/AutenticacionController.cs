using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class AutenticacionController : Controller
    {

        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public AutenticacionController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }


        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(UsuarioModel model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Autenticacion/Registrar";
                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;

                if(response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (result != null && result.Codigo == 0) 
                    {
                        return RedirectToAction("Ingresar", "Autenticacion");
                    }
                    else 
                    {
                        ViewBag.Mensaje = result!.Mensaje;
                        return View();
                    }
                }
            }
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
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Autenticacion/Ingresar";
                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<Respuesta>().Result;

                    if (result != null && result.Codigo == 0)
                    {
                        var datosContenido = JsonSerializer.Deserialize<UsuarioModel>((JsonElement)result.Contenido!);
                        HttpContext.Session.SetString("NombreUsuario", datosContenido!.Nombre);
                        return RedirectToAction("Index", "Pages");
                    }
                    else
                    {
                        ViewBag.Mensaje = result!.Mensaje;
                        return View();
                    }
                }
            }
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
