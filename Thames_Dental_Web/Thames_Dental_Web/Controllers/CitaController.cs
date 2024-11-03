using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    public class CitaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public CitaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _client = _httpClientFactory.CreateClient();

            // Configurar la dirección base de la API
            var apiUrl = _configuration.GetSection("Variables:RutaApi").Value;
            if (string.IsNullOrEmpty(apiUrl))
            {
                throw new Exception("La URL de la API no está configurada en appsettings.json.");
            }
            _client.BaseAddress = new Uri(apiUrl);
        }

        [HttpGet]
        public IActionResult Agendar()
        {
            return RedirectToAction("Index", "Pages");
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHorasOcupadas(string fecha)
        {
            var response = await _client.GetAsync($"Cita/ObtenerHorasOcupadas?fecha={fecha}");
            if (response.IsSuccessStatusCode)
            {
                var horasOcupadas = await response.Content.ReadFromJsonAsync<List<string>>();
                return Json(horasOcupadas);
            }
            else
            {
                return Json(new List<string>()); // Retorna lista vacía si hay un error
            }
        }



        [HttpPost]
        public async Task<IActionResult> Agendar(CitaModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {

                    TempData["SweetAlertMessage"] = "Datos de la cita no validos.";
                    TempData["SweetAlertType"] = "error";
                    return RedirectToAction("Index", "Pages");
                }

                // Asigna el especialista basado en la especialidad seleccionada
                model.Especialista = ObtenerEspecialistaPorEspecialidad(model.Especialidad);

                // Solicitar la duración del procedimiento desde la API
                var duracionResponse = await _client.GetAsync($"Cita/Duracion?procedimiento={model.Procedimiento}");
                if (duracionResponse.IsSuccessStatusCode)
                {
                    model.Duracion = int.Parse(await duracionResponse.Content.ReadAsStringAsync());
                }
                else
                {
                    model.Duracion = 60; // Valor predeterminado si no se obtiene duración
                }

                // Serializar el modelo de cita con la duración y el especialista incluidos
                var jsonContent = JsonContent.Create(model);

                // Realizar la solicitud POST a la API para agendar la cita
                var response = await _client.PostAsync("Cita/Agendar", jsonContent);

                // Procesar la respuesta de la API
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<Respuesta>();

                    if (result != null && result.Codigo == 0)
                    {
                        TempData["SweetAlertMessage"] = "Cita agendada correctamente.";
                        TempData["SweetAlertType"] = "success";
                    }
                    else
                    {
                        TempData["SweetAlertMessage"] = result?.Mensaje ?? "Error al agendar la cita.";
                        TempData["SweetAlertType"] = "error";
                    }
                }
                else
                {
                    TempData["SweetAlertMessage"] = $"Error al agendar la cita. Detalles: {response.ReasonPhrase}";
                    TempData["SweetAlertType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["SweetAlertMessage"] = $"Error: {ex.Message}";
                TempData["SweetAlertType"] = "error";
            }

            return RedirectToAction("Index", "Pages");
        }





        private string ObtenerEspecialistaPorEspecialidad(string especialidad)
        {
            var especialistas = new Dictionary<string, string>
    {
        { "Consulta General", "Dra. Stephanie Thames" },
        { "Peridoncia", "Dra. Stephanie Thames" },
        { "Endodoncia", "Dr. Josue Ulate" },
        { "Cirujia Maxilofacial", "Dr. Pablo Arguello" },
        { "Odontopediatria", "Dra. Karen Brenes" },
        { "ATM", "Dr. Pablo Arguello" },
        { "Ortodoncia", "Dra. Natalia Marenco" }
    };

            return especialistas.ContainsKey(especialidad) ? especialistas[especialidad] : "Especialista no asignado";
        }



        //Parte Administrativa 
        [HttpGet]
        public async Task<IActionResult> AdministrarCitas()
        {
            var response = await _client.GetAsync("Cita/ObtenerCitas");
            if (response.IsSuccessStatusCode)
            {
                var citas = await response.Content.ReadFromJsonAsync<List<CitaModel>>();
                return View("~/Views/Admin/AdministrarCitas.cshtml", citas);
            }
            else
            {
                TempData["SweetAlertMessage"] = "Error al cargar las citas.";
                TempData["SweetAlertType"] = "error";
                return View("~/Views/Admin/AdministrarCitas.cshtml", new List<CitaModel>());
            }
        }


        //Parte Administrativa


    }
}


