using Microsoft.AspNetCore.Mvc;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class ExpedientesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public ExpedientesController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }



        public IActionResult Expediente()
        {
            return View();
        }

        public IActionResult Autorizados()
        {
            return View();
        }

        public IActionResult Notas()
        {
            return View();
        }

        public IActionResult HistoriaClinica()
        {
            return View();
        }

        public IActionResult Odontograma()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Recetario()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Recetario(Receta model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expedientes/AgregarReceta";


                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;


                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Receta guardada correctamente .";
                }
                else
                {
                    ViewBag.Message = "Error al guardar la receta.";
                }

                return View(); // Recargas la misma vista

            }
        }

        public IActionResult Laboratorio()
        {
            return View();
        }
    }
}
