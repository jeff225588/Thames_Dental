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

        public IActionResult Dashboard()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Expediente()
        {
            return View();
        }


        // LLamar al api 
        [HttpPost]
        public IActionResult Expediente(ClientModel model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarCliente";


                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;


                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Cliente guardado exitosamente.";
                }
                else
                {
                    ViewBag.Message = "Error al guardar el cliente.";
                }

                return View(); // Recargas la misma vista

            }
        }



        [HttpGet]
        public async Task<IActionResult> ListaExpedientes()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/ListarClientes";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var expedientes = await response.Content.ReadFromJsonAsync<List<ClientModel>>();
                    return View("ListaExpedientes", expedientes); // Cargar la nueva vista con los datos
                }
                else
                {
                    ViewBag.Message = "Error al obtener la lista de expedientes.";
                    return View("ListaExpedientes", new List<ClientModel>()); // Retornar una lista vacía en caso de error
                }
            }
        }


        [HttpGet]
        public IActionResult Notas()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Notas(Nota model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarNota";


                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;


                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Nota guardada correctamente .";
                }
                else
                {
                    ViewBag.Message = "Error al guardar la nota.";
                }

                return View(); // Recargas la misma vista

            }
        }

        [HttpGet]
        public IActionResult VerNotaPorCedula()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerNotaPorCedula(string cedula)
        {
            // Verifica si la cédula está vacía
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Message = "La cédula no puede estar vacía.";
                return View(); // Regresa a la misma vista si la cédula es nula
            }

            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/verNota";
                // Crear el contenido JSON para enviar la cédula
                var content = new { Cedula = cedula };
                JsonContent datos = JsonContent.Create(content);

                // Realiza la solicitud POST a la API
                var response = await client.PostAsync(url, datos);

                if (response.IsSuccessStatusCode)
                {
                    // Suponiendo que la respuesta contiene las notas
                    var notas = await response.Content.ReadFromJsonAsync<List<Nota>>();
                    ViewBag.Notas = notas; // Guardar las notas en ViewBag para usarlas en la vista
                }
                else
                {
                    ViewBag.Message = "Error al recuperar las notas.";
                }
            }

            // Aquí siempre se devuelve la vista, así que no hay un camino sin retorno.
            return View();
        }

        public IActionResult PruebaOdonto()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Autorizados()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Autorizados(Autorizado model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarAutorizado";


                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;


                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Autorizado guardado correctamente .";
                }
                else
                {
                    ViewBag.Message = "Error al guardar la nota.";
                }

                return View(); // Recargas la misma vista

            }
        }




    }


}

