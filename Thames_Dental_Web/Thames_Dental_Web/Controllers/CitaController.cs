using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Net.Http;
using System.Text;
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
        private readonly GoogleCalendarService _calendarService;
        private readonly HttpClient _client;

        public CitaController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IEmailSender emailSender, GoogleCalendarService calendarService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _emailSender = emailSender;
            _calendarService = calendarService;
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
        public async Task<IActionResult> ObtenerHorasDisponibles(string fecha, int duracion)
        {
            try
            {
                // Llamar al API que obtiene las horas disponibles
                var response = await _client.GetAsync($"Cita/ObtenerHorasDisponibles?fecha={fecha}&duracion={duracion}");

                if (response.IsSuccessStatusCode)
                {
                    var horasDisponibles = await response.Content.ReadFromJsonAsync<List<TimeSpan>>();

                    // Convertir TimeSpan a formato "HH:mm"
                    var horasFormato = horasDisponibles.Select(hora => hora.ToString(@"hh\:mm")).ToList();

                    return Json(horasFormato);
                }
                else
                {
                    Console.WriteLine($"Error en API: {response.ReasonPhrase}");
                    return Json(new List<string>()); // Devuelve lista vacía si hay error
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ObtenerHorasDisponibles: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerHorasOcupadas(string fecha, string duracion)
        {
            var response = await _client.GetAsync($"Cita/ObtenerHorasOcupadas?fecha={fecha}&duracion={duracion}");
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
        public async Task<IActionResult> CitasActivas()
        {
            return await ObtenerCita("Activa");
        }

        [HttpGet]
        public async Task<IActionResult> CitasCanceladas()
        {
            return await ObtenerCita("Cancelada");
        }

        [HttpGet]
        public async Task<IActionResult> CitasConfirmadas()
        {
            return await ObtenerCita("Confirmada");
        }

        [HttpGet]
        public async Task<IActionResult> CitasCompletadas()
        {
            return await ObtenerCita("Completada");
        }

        private async Task<IActionResult> ObtenerCita(string estado)
        {
            var response = await _client.GetAsync($"Cita/ObtenerCita?estado={estado}");
            if (response.IsSuccessStatusCode)
            {
                var citas = await response.Content.ReadFromJsonAsync<List<CitaModel>>();
                return View($"~/Views/Admin/Citas{estado}.cshtml", citas);
            }
            else
            {
                TempData["SweetAlertMessage"] = $"Error al cargar las citas {estado.ToLower()}.";
                TempData["SweetAlertType"] = "error";
                return View($"~/Views/Admin/Citas{estado}.cshtml", new List<CitaModel>());
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

            return RedirectToAction("CitasActivas");
        }



        // Método para manejar la reprogramación de citas
        [HttpPost]
        public async Task<IActionResult> ReprogramarCita(int id, DateTime fecha, TimeSpan hora, int duracion)
        {
            try
            {
                var citaResponse = await _client.GetAsync($"Cita/ObtenerCita?id={id}");
                if (!citaResponse.IsSuccessStatusCode)
                {
                    TempData["SweetAlertMessage"] = "No se pudo obtener la información de la cita.";
                    TempData["SweetAlertType"] = "error";
                    return RedirectToAction("CitasActivas");
                }

                var model = await citaResponse.Content.ReadFromJsonAsync<CitaModel>();

                var request = new ReprogramarCitaRequest
                {
                    Id = id,
                    Fecha = fecha,
                    Hora = hora,
                    Duracion = duracion
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
);
                var response = await _client.PostAsync("Cita/ReprogramarCita", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<Respuesta>();

                    TempData["SweetAlertType"] = resultado.Codigo == 0 ? "success" : "error";
                    TempData["SweetAlertMessage"] = resultado.Mensaje;

                    if (resultado.Codigo == 0)
                    {
                        model.Fecha = fecha;
                        model.Hora = hora;

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
                }
                else
                {
                    TempData["SweetAlertType"] = "error";
                    TempData["SweetAlertMessage"] = $"Error al reprogramar la cita. Detalles: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                TempData["SweetAlertType"] = "error";
                TempData["SweetAlertMessage"] = "Error al reprogramar la cita en el servidor.";
                Console.WriteLine($"Error al reprogramar la cita: {ex.Message}");
            }

            return RedirectToAction("CitasActivas");
        }


        [HttpPost]
        public async Task<IActionResult> AceptarCita(int id)
        {
            Console.WriteLine("Starting AceptarCita process...");

            // Obtener los detalles de la cita desde la API
            var citaResponse = await _client.GetAsync($"Cita/ObtenerCita?id={id}");
            if (citaResponse.IsSuccessStatusCode)
            {
                var model = await citaResponse.Content.ReadFromJsonAsync<CitaModel>();

                if (model != null)
                {
                    try
                    {
                        // Construir fecha y hora de inicio y fin
                        DateTime startDateTime = model.Fecha.Date + model.Hora;
                        DateTime endDateTime = startDateTime.AddMinutes(model.Duracion);

                        Console.WriteLine("Calling GoogleCalendarService to add event...");
                        await _calendarService.AgregarEventoGoogleCalendarAsync(
                            summary: $"Cita con {model.NombreUsuario}",
                            description: $"Procedimiento: {model.Procedimiento}",
                            location: "Tejar de el Guarco, Cartago, Costa Rica",
                            startDateTime: startDateTime,
                            endDateTime: endDateTime
                        );

                        // Llamada al API para confirmar la cita
                        var confirmResponse = await _client.PostAsync($"Cita/ConfirmarCita?id={id}", null);
                        if (confirmResponse.IsSuccessStatusCode)
                        {
                            TempData["SweetAlertMessage"] = "Cita aceptada y añadida a Google Calendar.";
                            TempData["SweetAlertType"] = "success";
                        }
                        else
                        {
                            TempData["SweetAlertMessage"] = "Cita añadida a Google Calendar, pero no se pudo confirmar en el servidor.";
                            TempData["SweetAlertType"] = "warning";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al agregar cita al calendario: {ex.Message}");
                        TempData["SweetAlertMessage"] = "Error al añadir la cita a Google Calendar.";
                        TempData["SweetAlertType"] = "error";
                    }
                }
                else
                {
                    TempData["SweetAlertMessage"] = "Error al obtener los detalles de la cita.";
                    TempData["SweetAlertType"] = "error";
                }
            }
            else
            {
                TempData["SweetAlertMessage"] = "Error al aceptar la cita.";
                TempData["SweetAlertType"] = "error";
            }

            Console.WriteLine("Finished AceptarCita process.");
            return RedirectToAction("CitasActivas");
        }


        [HttpPost]
        public async Task<IActionResult> CompletarCita(int id)
        {
            var response = await _client.PostAsync($"Cita/CompletarCita?id={id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SweetAlertMessage"] = "Cita completada exitosamente.";
                TempData["SweetAlertType"] = "success";
            }
            else
            {
                TempData["SweetAlertMessage"] = "Error al completar la cita.";
                TempData["SweetAlertType"] = "error";
            }

            return RedirectToAction("CitasCompletadas"); // Puedes cambiar esta acción según lo necesites
        }

        [HttpGet]
        public IActionResult FormularioAgendar()
        {
            return View($"~/Views/Admin/FormularioAgendar.cshtml");
            // Correcto, la vista se llama FormularioAgendar
        }



        [HttpPost]
        public async Task<IActionResult> AAgendar(CitaModel model)
        {

            try
            {
                if (!ModelState.IsValid)
                {

                    TempData["SweetAlertMessage"] = "Datos de la cita no validos.";
                    TempData["SweetAlertType"] = "error";
                    return RedirectToAction("CitasActiva", "Admin");
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

            return RedirectToAction("CitasActivas");
        }


        [HttpPost]
        public async Task<IActionResult> EditarDuracion(int Id, int Duracion)
        {
            Console.WriteLine($"Enviando solicitud para editar la duración de la cita con Id: {Id}");

            try
            {
                // Preparar el contenido JSON para la solicitud
                var content = new StringContent(
                    JsonSerializer.Serialize(new { id = Id, duracion = Duracion }),
                    Encoding.UTF8,
                    "application/json"
                );

                // Realizar la solicitud al API
                var response = await _client.PutAsync($"Cita/EditarDuracion", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SweetAlertMessage"] = "Duración actualizada exitosamente.";
                    TempData["SweetAlertType"] = "success";
                }
                else
                {
                    TempData["SweetAlertMessage"] = "Error al actualizar la duración.";
                    TempData["SweetAlertType"] = "error";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar la duración: {ex.Message}");
                TempData["SweetAlertMessage"] = $"Error interno: {ex.Message}";
                TempData["SweetAlertType"] = "error";
            }

            return RedirectToAction("CitasActivas");
        }



        //Parte Administrativa



    }
}




