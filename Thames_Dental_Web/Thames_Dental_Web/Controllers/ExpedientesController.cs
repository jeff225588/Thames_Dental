using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Thames_Dental_Web.Filters;
using Thames_Dental_Web.Models;

namespace Thames_Dental_Web.Controllers
{
    [AdminFilter]
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

        [HttpPost("Expedientes/EliminarExpediente/{ClienteID}")]
        public async Task<IActionResult> EliminarExpediente(int ClienteID)
        {
            using (var client = _http.CreateClient())
            {
                // URL que apunta a tu API
                string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/EliminarExpediente/{ClienteID}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Expediente eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al eliminar el expediente.";
                }

                // Redirige nuevamente a la lista de expedientes después de eliminar
                return RedirectToAction("ListaExpedientes");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditarExpediente(int clienteID)
        {
            ClientModel? model = null;  // Usar el operador de tipo nullable "?"

            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerCliente/{clienteID}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        model = await response.Content.ReadFromJsonAsync<ClientModel>();
                    }
                    else
                    {
                        ViewBag.Message = "Error al cargar los datos del cliente.";
                        return View("Error");
                    }
                }

                if (model == null)  // Si model es nulo, maneja el caso
                {
                    ViewBag.Message = "Cliente no encontrado.";
                    return View("Error");
                }

                return View(model);  // Aquí se pasa el modelo a la vista
            }
            catch (Exception ex)
            {
                // Registra el error o envíalo a la vista
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }



        [HttpPost]
        public IActionResult EditarExpediente(ClientModel model)
        {
            using (var client = _http.CreateClient())
            {
                // Construir la URL para llamar al método EditarExpediente en el API
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/EditarExpediente";

                // Crear el contenido JSON a partir del modelo para enviarlo en el cuerpo de la solicitud
                JsonContent datos = JsonContent.Create(model);

                // Realizar una solicitud POST a la URL especificada con el contenido JSON
                var response = client.PostAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Expediente actualizado exitosamente.";
                }
                else
                {
                    ViewBag.Message = "Error al actualizar el expediente.";
                }

                return RedirectToAction("ListaExpedientes"); // Redirigir a la vista de lista de expedientes
            }
        }


        [HttpGet]
        public async Task<IActionResult> VerCliente(int clienteID)
        {
            ClientModel? model = null;  // Utilizar el tipo nullable "?"

            try
            {
                using (var client = _http.CreateClient())
                {
                    // Construir la URL para obtener los datos del cliente en la API
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerCliente/{clienteID}";

                    // Realizar una solicitud GET a la URL especificada
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Leer el contenido de la respuesta JSON y asignarlo al modelo
                        model = await response.Content.ReadFromJsonAsync<ClientModel>();
                    }
                    else
                    {
                        // Si la respuesta no es exitosa, mostrar un mensaje de error
                        ViewBag.Message = "Error al cargar los datos del cliente.";
                        return View("Error");
                    }
                }

                // Si no se encuentra el cliente, manejar el caso de datos nulos
                if (model == null)
                {
                    ViewBag.Message = "Cliente no encontrado.";
                    return View("Error");
                }

                // Retornar la vista con el modelo del cliente
                return View("VerCliente", model);  // Cambiar a la vista "VerCliente" para mostrar los datos
            }
            catch (Exception ex)
            {
                // Manejar excepciones y enviar un mensaje de error a la vista
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
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


        [HttpGet]
        public IActionResult Recetario()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AgregarReceta()
        {
            return View();
        }


        [HttpPost]
        public IActionResult AgregarReceta(RecetaRequest model)
        {
            using (var client = _http.CreateClient())
            {
                // Cambia la URL al endpoint correspondiente en tu API
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarReceta";

                // Crear el contenido JSON de la solicitud
                JsonContent datos = JsonContent.Create(model);

                // Enviar la solicitud POST al API
                var response = client.PostAsync(url, datos).Result;

                // Verificar la respuesta
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Receta guardada correctamente.";
                }
                else
                {
                    ViewBag.Message = "Error al guardar la receta: " + response.Content.ReadAsStringAsync().Result;
                }

                return View(); // Recargar la misma vista o redirigir a otra vista si es necesario
            }
        }






        [HttpGet]
        public IActionResult HistoriaClinica()
        {
            return View();
        }


        [HttpPost]
        public IActionResult HistoriaClinica(HistorialClinicoRequest model)
        {
            using (var client = _http.CreateClient())
            {
                // URL del endpoint de la API
                string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarHistorialClinico";

                // Crear el contenido JSON de la solicitud
                JsonContent datos = JsonContent.Create(model);

                // Enviar la solicitud POST al API
                var response = client.PostAsync(url, datos).Result;

                // Verificar la respuesta
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Historial clínico guardado correctamente.";
                }
                else
                {
                    ViewBag.Message = "Error al guardar el historial clínico: " + response.Content.ReadAsStringAsync().Result;
                }

                return View(); // Recargar la misma vista o redirigir a otra vista si es necesario
            }
        }


        [HttpGet]
        public async Task<IActionResult> VerRecetas(int clienteID)
        {
            ClientModel model = new ClientModel(); // Inicializa el modelo
            try
            {
                using (var client = _http.CreateClient())
                {
                    // Construir la URL para obtener las recetas del cliente
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerRecetasPorCliente/{clienteID}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Asignar las recetas al modelo, garantizando que no sea null
                        model.Recetas = await response.Content.ReadFromJsonAsync<List<RecetaRequest>>() ?? new List<RecetaRequest>();
                        model.ClienteID = clienteID; // Asignar ClienteID correctamente
                    }
                    else
                    {
                        model.ClienteID = clienteID;
                        ViewBag.Message = "Error al cargar las recetas del cliente.";
                        return View(model);
                    }
                }

                // Verifica si no se encontraron recetas
                if (model.Recetas == null || !model.Recetas.Any())
                {
                    ViewBag.Message = "Este usuario no tiene recetas.";
                    return View(model); // Pasa el modelo con el mensaje adecuado
                }

                // Si hay recetas, pasa el modelo a la vista
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }





        [HttpGet]
        public IActionResult AgregarRecetaporid(int ClienteID)
        {
            // Si el ClienteID es inválido, podrías redirigir a una página de error o mostrar un mensaje
            if (ClienteID <= 0)
            {
                return BadRequest("El ClienteID no es válido.");
            }

            // Crear una instancia del modelo RecetaRequest con el ClienteID
            var recetaRequest = new RecetaRequest
            {
                ClienteID = ClienteID
            };

            // Pasar el modelo a la vista
            return View(recetaRequest);
        }




        [HttpPost]
        public async Task<IActionResult> AgregarRecetaporid(RecetaRequest recetaRequest)
        {
            if (recetaRequest.ClienteID <= 0 || string.IsNullOrEmpty(recetaRequest.Medicamento))
            {
                return BadRequest(new { success = false, message = "El ClienteID o Medicamento no son válidos." });
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarRecetaporid";
                    var response = await client.PostAsJsonAsync(url, recetaRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { success = true, message = "Receta agregada con éxito." });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return BadRequest(new { success = false, message = errorMessage });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error inesperado: {ex.Message}" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> VerAutorizados(int clienteID)
        {
            ClientModel model = new ClientModel(); // Inicializa el modelo
            try
            {
                using (var client = _http.CreateClient())
                {
                    // Construir la URL para obtener los autorizados del cliente
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerAutorizadosPorClienteID/{clienteID}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(jsonResponse);  // Esto te ayudará a ver la respuesta JSON
                        model.Autorizado = await response.Content.ReadFromJsonAsync<List<Autorizado>>() ?? new List<Autorizado>();
                        model.ClienteID = clienteID;
                    }
                    else
                    {
                        model.ClienteID = clienteID;
                        ViewBag.Message = "Error al cargar los autorizados del cliente.";
                        return View(model);
                    }
                }



                // Verifica si no se encontraron autorizados
                if (model.Autorizado == null || !model.Autorizado.Any())
                {
                    ViewBag.Message = "Este usuario no tiene autorizados.";
                    return View(model); // Pasa el modelo con el mensaje adecuado
                }

                // Si hay autorizados, pasa el modelo a la vista
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }


        [HttpGet]
        public IActionResult AgregarAutorizadoporid(int ClienteID)
        {
            if (ClienteID <= 0)
            {
                return BadRequest("El ClienteID no es válido.");
            }

            var model = new AgregarAutorizadoViewModel
            {
                ClienteID = ClienteID
            };

            return View(model);
        }

        // Acción POST para agregar un autorizado
        [HttpPost]
        public async Task<IActionResult> AgregarAutorizadoporid(AgregarAutorizadoViewModel model)
        {
            // Validar que el ClienteID y el Nombre sean válidos
            if (model.ClienteID <= 0 || string.IsNullOrEmpty(model.Nombre))
            {
                model.ErrorMessage = "El ClienteID o Nombre no son válidos.";
                return View(model);
            }

            try
            {
                // Crear cliente HTTP para realizar la solicitud POST a la API
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarAutorizadoporid";

                    var response = await client.PostAsJsonAsync(url, model);

                    if (response.IsSuccessStatusCode)
                    {
                        var successMessage = await response.Content.ReadAsStringAsync();
                        model.SuccessMessage = "Autorizado agregado con éxito.";

                        // Redirigir a la página "VerAutorizados" después de guardar
                        return RedirectToAction("VerAutorizados", "Expedientes", new { clienteID = model.ClienteID });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        model.ErrorMessage = "Error al agregar el autorizado: " + errorMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "Error inesperado: " + ex.Message;
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> VerHistorialClinicoporid(int clienteID)
        {
            ClientModel model = new ClientModel(); // Inicializa el modelo
            try
            {
                using (var client = _http.CreateClient())
                {
                    // Construir la URL para obtener el historial clínico del cliente
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerHistorialClinicoPorClienteID/{clienteID}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Asignar el historial clínico al modelo
                        model.HistorialClinicoporid = await response.Content.ReadFromJsonAsync<List<HistorialClinicoporid>>() ?? new List<HistorialClinicoporid>();
                        model.ClienteID = clienteID; // Asignar ClienteID correctamente
                    }
                    else
                    {
                        model.ClienteID = clienteID;
                        ViewBag.Message = "Error al cargar el historial clínico del cliente.";
                        return View(model);
                    }
                }

                // Verifica si no se encontró historial clínico
                if (model.HistorialClinicoporid == null || !model.HistorialClinicoporid.Any())
                {
                    ViewBag.Message = "Este cliente no tiene historial clínico registrado.";
                    return View(model); // Pasa el modelo con el mensaje adecuado
                }

                // Si hay historial clínico, pasa el modelo a la vista
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }


        [HttpGet]
        public IActionResult AgregarHistorialClinicoPorId(int ClienteID)
        {
            if (ClienteID <= 0)
            {
                return BadRequest("El ClienteID no es válido.");
            }

            var historialModel = new ClientModel
            {
                ClienteID = ClienteID

            };

            return View(historialModel);
        }


        [HttpPost]
        public async Task<IActionResult> AgregarHistorialClinicoPorId(HistorialClinicoRequest ClientModel)
        {
            // Validación de los datos de entrada
            if (ClientModel.ClienteID <= 0)
            {
                TempData["ErrorMessage"] = "El ClienteID no es válido.";
                return RedirectToAction("AgregarHistorialClinicoPorId", new { ClienteID = ClientModel.ClienteID });
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    // URL del API que llamará al procedimiento almacenado
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarHistorialClinicoPorId";

                    var response = await client.PostAsJsonAsync(url, ClientModel);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Historial clínico agregado con éxito.";
                        return RedirectToAction("VerHistorialClinicoporid", new { ClienteID = ClientModel.ClienteID });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al agregar historial clínico: " + errorMessage;
                        return RedirectToAction("AgregarHistorialClinicoPorId", new { ClienteID = ClientModel.ClienteID });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("AgregarHistorialClinicoPorId", new { ClienteID = ClientModel.ClienteID });
            }
        }


        [HttpGet]
        public async Task<IActionResult> VerNotasPorID(int clienteID)
        {
            // Modelo para pasar los datos a la vista
            ClientModel model = new ClientModel();

            try
            {
                using (var client = _http.CreateClient())
                {
                    // Construir la URL del API
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/VerNotasPorID/{clienteID}";

                    // Realizar la solicitud al API
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Deserializar las notas obtenidas del API
                        model.Nota = await response.Content.ReadFromJsonAsync<List<Nota>>() ?? new List<Nota>();
                        model.ClienteID = clienteID;
                    }
                    else
                    {
                        // Manejo de errores si la respuesta no es exitosa
                        model.ClienteID = clienteID;
                        ViewBag.Message = "Error al cargar las notas del cliente.";
                        return View(model);
                    }
                }

                // Si no se encuentran notas
                if (model.Nota == null || !model.Nota.Any())
                {
                    ViewBag.Message = "Este cliente no tiene notas registradas.";
                    return View(model);
                }

                // Pasar el modelo a la vista si hay notas
                return View(model);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                ViewBag.Message = $"Error inesperado: {ex.Message}";
                return View("Error");
            }
        }


        [HttpGet]
        public IActionResult AgregarNotaporid(int clienteID)
        {
            // Verifica si el ClienteID es válido
            if (clienteID <= 0)
            {
                // Puedes manejar un error si el ClienteID no es válido
                TempData["ErrorMessage"] = "ClienteID no válido.";
                return RedirectToAction("Index"); // O redirige a la página principal, según lo que desees
            }

            // Crear un objeto Nota con el ClienteID
            var nota = new Nota
            {
                ClienteID = clienteID
            };

            // Retorna la vista con el modelo que contiene el ClienteID
            return View(nota);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarNotaporid(Nota ClientModel)
        {
            // Validación de los datos de entrada
            if (ClientModel.ClienteID <= 0 || string.IsNullOrEmpty(ClientModel.Detalle))
            {
                TempData["ErrorMessage"] = "El ClienteID o Detalle no son válidos.";
                return RedirectToAction("VerNotasPorID", new { ClienteID = ClientModel.ClienteID });
            }

            try
            {
                // Crear un cliente HTTP para realizar la solicitud
                using (var client = _http.CreateClient())
                {
                    // URL del API que llamará al procedimiento almacenado
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/AgregarNotasPorID";

                    // Realizar la solicitud POST al API con el modelo Nota
                    var response = await client.PostAsJsonAsync(url, ClientModel);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Nota agregada con éxito.";
                        return RedirectToAction("VerNotasPorID", new { ClienteID = ClientModel.ClienteID });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al agregar la nota: " + errorMessage;
                        return RedirectToAction("VerNotasPorID", new { ClienteID = ClientModel.ClienteID });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("VerNotasPorID", new { ClienteID = ClientModel.ClienteID });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarEstadoExpediente(int clienteId, bool nuevoEstado)
        {
            if (clienteId <= 0)
            {
                TempData["ErrorMessage"] = "ClienteID no válido.";
                return RedirectToAction("ListaExpedientes");
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    // URL del API que actualizará el estado del expediente
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/ActualizarEstadoExpediente";

                    // Crear el modelo que se enviará en la solicitud
                    var payload = new
                    {
                        ClienteID = clienteId,
                        NuevoEstado = nuevoEstado
                    };

                    // Realizar la solicitud POST al API
                    var response = await client.PostAsJsonAsync(url, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = $"El expediente fue {(nuevoEstado ? "activado" : "desactivado")} con éxito.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error al actualizar el estado del expediente.";
                    }

                    return RedirectToAction("ListaExpedientes");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("ListaExpedientes");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerExpedientesDesactivados()
        {
            try
            {
                // Crear un cliente HTTP para realizar la solicitud
                using (var client = _http.CreateClient())
                {
                    // URL del API que obtendrá los expedientes desactivados
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/ObtenerExpedientesDesactivados";

                    // Realizar la solicitud GET al API
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        // Leer y deserializar la respuesta
                        var expedientes = await response.Content.ReadFromJsonAsync<List<ClientModel>>();

                        // Pasar los datos a la vista
                        return View(expedientes);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error al obtener los expedientes desactivados.";
                        return RedirectToAction("Index"); // O redirige según lo que necesites
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCotizacionesPorID(int clienteId)
        {
            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Expediente/ObtenerCotizacionesPorID/{clienteId}";

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var cotizaciones = await response.Content.ReadFromJsonAsync<List<CotizacionModel>>();

                        if (cotizaciones == null || !cotizaciones.Any())
                        {
                            TempData["InfoMessage"] = "Este cliente no tiene cotizaciones. Puedes agregar una nueva.";
                            return RedirectToAction("AgregarCotizacion", new { clienteID = clienteId });
                        }

                        ViewBag.ClienteID = clienteId; // Pasar clienteId a la vista
                        return View(cotizaciones);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Error al obtener las cotizaciones del cliente.";
                        ViewBag.ClienteID = clienteId; // Pasar clienteId a la vista incluso en caso de error
                        return View(new List<CotizacionModel>());
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error inesperado: " + ex.Message;
                ViewBag.ClienteID = clienteId; // Pasar clienteId para casos de excepción
                return View(new List<CotizacionModel>());
            }
        }




        [HttpGet]
        public IActionResult AgregarCotizacion(int clienteID)
        {
            if (clienteID <= 0)
            {
                TempData["ErrorMessage"] = "ClienteID no válido.";
                return RedirectToAction("ListaExpedientes");
            }

            var cotizacion = new CotizacionModel
            {
                ClienteID = clienteID
            };

            return View(cotizacion); // Retorna la vista con el modelo vacío
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCotizacion(CotizacionModel cotizacion)
        {
            if (cotizacion.ClienteID <= 0 || string.IsNullOrEmpty(cotizacion.TextoCotizacion))
            {
                TempData["ErrorMessage"] = "Datos inválidos para la cotización.";
                return View(cotizacion);
            }

            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + "Expediente/InsertarCotizacion";

                    var response = await client.PostAsJsonAsync(url, cotizacion);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Cotización agregada con éxito.";
                        return RedirectToAction("ObtenerCotizacionesPorID", new { clienteID = cotizacion.ClienteID });
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = "Error al agregar la cotización: " + errorMessage;
                        return View(cotizacion);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error inesperado: " + ex.Message;
                return View(cotizacion);
            }
        }








    }


}

