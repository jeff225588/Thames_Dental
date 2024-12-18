using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Thames_Dental_API.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Thames_Dental_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitaController : ControllerBase
    {
        private readonly IConfiguration _conf;
        private readonly IHostEnvironment _env;

        public CitaController(IConfiguration conf, IHostEnvironment env)
        {
            _conf = conf;
            _env = env;
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

        [HttpGet("Duracion")]
        public IActionResult ObtenerDuracionProcedimiento(string procedimiento)
        {
            var duraciones = new Dictionary<string, int>
    {
        { "Limpieza dental", 60 },
        { "Blanqueamiento", 90 },
        { "Ortodoncia", 120 },
        { "Biopsia", 60 },
        { "Consulta general", 45 }
    };

            int duracion = duraciones.ContainsKey(procedimiento) ? duraciones[procedimiento] : 60; // Duración por defecto de 1h
            return Ok(duracion);
        }



        [HttpGet("ObtenerHorasOcupadas")]
        public async Task<IActionResult> ObtenerHorasOcupadas(string fecha)
        {
            Console.WriteLine($"Fecha recibida: {fecha}");
            if (!DateTime.TryParse(fecha, out DateTime fechaSeleccionada))
            {
                return BadRequest("Fecha no válida.");
            }

            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var horasOcupadas = await connection.QueryAsync<string>(
                        @"SELECT FORMAT(Hora, 'HH:mm') 
                FROM Citas 
                WHERE Fecha = @Fecha",
                        new { Fecha = fechaSeleccionada });

                    Console.WriteLine("Horas ocupadas obtenidas de la base de datos:");
                    foreach (var hora in horasOcupadas)
                    {
                        Console.WriteLine(hora);
                    }

                    return Ok(horasOcupadas);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener horas ocupadas: {ex.Message}");
                return StatusCode(500, "Error al obtener las horas ocupadas.");
            }
        }


        [HttpGet("ObtenerHorasDisponibles")]
        public async Task<IActionResult> ObtenerHorasDisponibles(string fecha, int duracion)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var horasDisponibles = await connection.QueryAsync<TimeSpan>(
                        "ObtenerHorasDisponibles",
                        new { Fecha = DateTime.Parse(fecha), Duracion = duracion },
                        commandType: CommandType.StoredProcedure);

                    return Ok(horasDisponibles);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener horas disponibles: {ex.Message}");
                return StatusCode(500, "Error al obtener las horas disponibles.");
            }
        }






        [HttpPost("Agendar")]
        public async Task<IActionResult> Agendar(Cita model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Respuesta { Codigo = -1, Mensaje = "Datos de la cita no validos." });
            }

            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;
            var respuesta = new Respuesta();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(); // Aseguramos que la conexión esté abierta

                    // Calcular hora de fin
                    var horaInicioNueva = model.Hora;
                    var horaFinNueva = model.Hora.Add(TimeSpan.FromMinutes(model.Duracion));

                    // Log para depuración
                    Console.WriteLine($"Hora Inicio: {horaInicioNueva}, Hora Fin: {horaFinNueva}");

                    // Llamar al SP para verificar si existe un conflicto de horario
                    try
                    {
                        var parametrosVerificar = new
                        {
                            Especialista = model.Especialista,
                            Fecha = model.Fecha,
                            HoraInicioNueva = horaInicioNueva,
                            HoraFinNueva = horaFinNueva
                        };

                        var consultaExistente = await connection.QueryFirstOrDefaultAsync<int>(
                            "VerificarCitaExistente", parametrosVerificar, commandType: CommandType.StoredProcedure);

                        Console.WriteLine($"Resultado VerificarCitaExistente: {consultaExistente}");

                        if (consultaExistente > 0)
                        {
                            respuesta.Codigo = -1;
                            respuesta.Mensaje = "El especialista ya tiene una cita en este horario.";
                            return Ok(respuesta);
                        }
                    }
                    catch (Exception exVerificar)
                    {
                        Console.WriteLine($"Error en VerificarCitaExistente: {exVerificar.Message}");
                        throw;
                    }

                    // Llamar al SP para insertar la nueva cita
                    try
                    {
                        var parametrosInsertar = new
                        {
                            model.NombreUsuario,
                            model.Email,
                            model.Especialidad,
                            model.Especialista,
                            model.Fecha,
                            model.Hora,
                            model.Procedimiento,
                            model.Duracion
                        };

                        await connection.ExecuteAsync("InsertarCita", parametrosInsertar, commandType: CommandType.StoredProcedure);

                        Console.WriteLine("Cita insertada exitosamente en la base de datos.");

                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Cita agendada correctamente.";
                        return Ok(respuesta);
                    }
                    catch (Exception exInsertar)
                    {
                        Console.WriteLine($"Error en InsertarCita: {exInsertar.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al agendar la cita: {ex.Message}";
                return StatusCode(500, respuesta);
            }
        }







        //Parte Administrativa 
        [HttpGet("ObtenerCita")]
        public async Task<IActionResult> ObtenerCita(int? id = null, string estado = null)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Si se proporciona un ID, buscará una cita específica
                    if (id.HasValue)
                    {
                        var query = "SELECT * FROM Citas WHERE Id = @Id";
                        var cita = await connection.QueryFirstOrDefaultAsync<Cita>(query, new { Id = id.Value });

                        if (cita == null)
                            return NotFound("Cita no encontrada");

                        return Ok(cita);
                    }
                    // Si se proporciona un estado, filtrar citas por estado
                    else if (!string.IsNullOrEmpty(estado))
                    {
                        var query = "SELECT * FROM Citas WHERE Estado = @Estado";
                        var citas = await connection.QueryAsync<Cita>(query, new { Estado = estado });

                        return Ok(citas);
                    }
                    else
                    {
                        return BadRequest("Debe proporcionar un Id o un estado.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener citas: {ex.Message}");
                return StatusCode(500, "Error al obtener las citas.");
            }
        }


        [HttpPost("CancelarCita")]
        public async Task<IActionResult> CancelarCita(int id)
        {
            Console.WriteLine($"Id recibido en la API para cancelar cita: {id}");
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Ejecutar el SP
                    var parameters = new { Id = id };
                    var cita = await connection.QueryFirstOrDefaultAsync<Cita>(
                        "CancelarCita",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (cita == null)
                    {
                        return NotFound(); // 404 si no se encontró la cita
                    }

                    return Ok(cita); // Devuelve los detalles de la cita cancelada
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cancelar la cita: {ex.Message}");
                return StatusCode(500); // 500 en caso de un error del servidor
            }
        }




        [HttpPost("ReprogramarCita")]
        public async Task<IActionResult> ReprogramarCita(int id, DateTime fecha, TimeSpan hora)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = "UPDATE Citas SET Fecha = @Fecha, Hora = @Hora WHERE Id = @Id";
                    var affectedRows = await connection.ExecuteAsync(query, new { Fecha = fecha, Hora = hora, Id = id });

                    if (affectedRows > 0)
                    {
                        return Ok("Cita reprogramada correctamente.");
                    }
                    else
                    {
                        return NotFound("Cita no encontrada.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reprogramar la cita: {ex.Message}");
                return StatusCode(500, $"Error al reprogramar la cita: {ex.Message}");
            }

        }

        [HttpPost("ConfirmarCita")]
        public async Task<IActionResult> ConfirmarCita(int id)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Llamar al SP ConfirmarCita
                    var parameters = new { Id = id };
                    var result = await connection.ExecuteScalarAsync<int>(
                        "ConfirmarCita",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == -1)
                    {
                        return NotFound("Cita no encontrada");
                    }

                    return Ok("Cita confirmada correctamente");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al confirmar la cita: {ex.Message}");
                return StatusCode(500, "Error al confirmar la cita");
            }
        }


        [HttpPost("CompletarCita")]
        public async Task<IActionResult> CompletarCita(int id)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Llamar al SP CompletarCita
                    var parameters = new { Id = id };
                    var result = await connection.ExecuteScalarAsync<int>(
                        "CompletarCita",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == -1)
                    {
                        return NotFound("Cita no encontrada.");
                    }

                    return Ok("Cita completada exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al completar la cita: {ex.Message}");
                return StatusCode(500, "Error al completar la cita.");
            }
        }






        [HttpPost("AAgendar")]
        public async Task<IActionResult> AAgendar(Cita model)
        {
             
            if (!ModelState.IsValid)
            {
                return BadRequest(new Respuesta { Codigo = -1, Mensaje = "Datos de la cita no validos." });
            }
            // Obtener la cadena de conexión desde la configuración
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;
            var respuesta = new Respuesta();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    // Verificar si existe una cita en la misma fecha y hora con el mismo especialista
                    var consultaExistente = await connection.QueryFirstOrDefaultAsync<int>(
                        "SELECT COUNT(1) FROM Citas WHERE Especialista = @Especialista AND Fecha = @Fecha AND Hora = @Hora",
                        new { model.Especialista, model.Fecha, model.Hora });

                    if (consultaExistente > 0)
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "El especialista ya tiene una cita en la misma fecha y hora.";
                        return Ok(respuesta); // Retornamos Ok con el mensaje de error estructurado
                    }

                    // Agregar la cita
                    var query = @"INSERT INTO Citas (NombreUsuario, Email, Especialidad, Especialista, Fecha, Hora, Procedimiento, Duracion) 
                                  VALUES (@NombreUsuario, @Email, @Especialidad, @Especialista, @Fecha, @Hora, @Procedimiento, @Duracion)";

                    await connection.ExecuteAsync(query, model);

                    respuesta.Codigo = 0;
                    respuesta.Mensaje = "Cita agendada correctamente.";
                    return Ok(respuesta); // Retornamos Ok con el mensaje de éxito
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Mensaje = $"Error al agendar la cita: {ex.Message}";
                if (ex.InnerException != null)
                {
                    respuesta.Mensaje += $" - InnerException: {ex.InnerException.Message}";
                }
                Console.WriteLine(ex.Message);
                return StatusCode(500, respuesta);
            }
        }

        [HttpPut("EditarDuracion")]
        public async Task<IActionResult> EditarDuracion([FromBody] EditarDuracionRequest request)
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var parameters = new { Id = request.Id, Duracion = request.Duracion };
                    var result = await connection.ExecuteScalarAsync<int>(
                        "EditarDuracionCita",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result == -1)
                    {
                        return NotFound(new { success = false, message = "La cita no existe." });
                    }

                    return Ok(new { success = true, message = "Duración actualizada exitosamente." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor.", details = ex.Message });
            }
        }
    }

    public class EditarDuracionRequest
    {
        public int Id { get; set; }
        public int Duracion { get; set; }
    }






    //Parte Administrativa 
}


