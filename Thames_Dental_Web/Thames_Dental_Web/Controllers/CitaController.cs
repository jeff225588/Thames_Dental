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
        private readonly IEmailSender _emailSender;
        private readonly HttpClient _client;

        public CitaController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IEmailSender emailSender)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _emailSender = emailSender;
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
                        // Enviar correo al usuario
                        await _emailSender.SendEmailAsync(
                            model.Email,
                            "Confirmación de Cita - Thames Dental",
                            model.NombreUsuario,
                            model.Fecha.ToString("dd/MM/yyyy"),
                            model.Hora.ToString(@"hh\:mm"),
                            model.Especialidad,
                            model.Procedimiento,
                            model.Especialista
                        );
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

        [HttpPost]
        public async Task<IActionResult> CancelarCita(int id)
        {
            Console.WriteLine($"Enviando solicitud de cancelación para Id: {id}");

            // Obtener los detalles de la cita desde la API o base de datos
            var citaResponse = await _client.GetAsync($"Cita/ObtenerCita?id={id}");

            // Envía la solicitud de cancelación
            var response = await _client.PostAsync($"Cita/CancelarCita?id={id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SweetAlertMessage"] = "Cita cancelada correctamente.";
                TempData["SweetAlertType"] = "success";


                if (citaResponse.IsSuccessStatusCode)
                {
                    var model = await citaResponse.Content.ReadFromJsonAsync<CitaModel>();

                    // Enviar correo de notificación de cancelación
                    if (model != null)
                    {
                        await _emailSender.SendEmailAsync(
                            model.Email,
                            "Cancelación de Cita - Thames Dental",
                            model.NombreUsuario,
                            model.Fecha.ToString("dd/MM/yyyy"),
                            model.Hora.ToString(@"hh\:mm"),
                            model.Especialidad,
                            model.Procedimiento,
                            model.Especialista,
                            isCancellation: true
                        );
                    }
                }
            }
            else
            {
                TempData["SweetAlertMessage"] = "Error al cancelar la cita en el servidor.";
                TempData["SweetAlertType"] = "error";
            }

            return RedirectToAction("AdministrarCitas");
        }



        // Método para manejar la reprogramación de citas
        [HttpPost]
        public async Task<IActionResult> ReprogramarCita(int id, DateTime fecha, TimeSpan hora)
        {
            try
            {
                // Obtener detalles de la cita antes de reprogramar
                var citaResponse = await _client.GetAsync($"Cita/ObtenerCita?id={id}");
                if (!citaResponse.IsSuccessStatusCode)
                {
                    TempData["SweetAlertMessage"] = "No se pudo obtener la información de la cita.";
                    TempData["SweetAlertType"] = "error";
                    return RedirectToAction("AdministrarCitas");
                }

                var model = await citaResponse.Content.ReadFromJsonAsync<CitaModel>();

                // Enviar solicitud para reprogramar la cita
                var response = await _client.PostAsync($"Cita/ReprogramarCita?id={id}&fecha={fecha:yyyy-MM-dd}&hora={hora}", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SweetAlertMessage"] = "Cita reprogramada correctamente.";
                    TempData["SweetAlertType"] = "success";

                    // Actualizar la fecha y hora en el modelo
                    model.Fecha = fecha;
                    model.Hora = hora;

                    // Enviar correo de notificación de reprogramación
                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Reprogramación de Cita - Thames Dental",
                        model.NombreUsuario,
                        model.Fecha.ToString("dd/MM/yyyy"),
                        model.Hora.ToString(@"hh\:mm"),
                        model.Especialidad,
                        model.Procedimiento,
                        model.Especialista,
                        isReschedule: true
                    );
                }
                else
                {
                    TempData["SweetAlertMessage"] = $"Error al reprogramar la cita. Detalles: {response.ReasonPhrase}";
                    TempData["SweetAlertType"] = "error";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reprogramar la cita: {ex.Message}");
                TempData["SweetAlertMessage"] = "Error al reprogramar la cita en el servidor.";
                TempData["SweetAlertType"] = "error";
            }

            return RedirectToAction("AdministrarCitas");
        }




        //Parte Administrativa


    }
}


