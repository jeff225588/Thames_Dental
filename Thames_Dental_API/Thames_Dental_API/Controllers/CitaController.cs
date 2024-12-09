using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Thames_Dental_API.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Data;

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





        [HttpPost("Agendar")]
        public async Task<IActionResult> Agendar(Cita model)
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
                    // Verificar si la cita existe antes de actualizar
                    var cita = await connection.QueryFirstOrDefaultAsync<Cita>("SELECT * FROM Citas WHERE Id = @Id", new { Id = id });

                    if (cita == null)
                    {
                        return NotFound("Cita no encontrada");
                    }

                    // Actualizar el estado de la cita a "Confirmada"
                    var updateQuery = "UPDATE Citas SET Estado = @Estado WHERE Id = @Id";
                    var affectedRows = await connection.ExecuteAsync(updateQuery, new { Estado = "Confirmada", Id = id });

                    if (affectedRows > 0)
                    {
                        return Ok("Cita confirmada correctamente");
                    }
                    else
                    {
                        return StatusCode(500, "Error al actualizar el estado de la cita");
                    }
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
                    var query = "UPDATE Citas SET Estado = @Estado WHERE Id = @Id";
                    var result = await connection.ExecuteAsync(query, new { Estado = "Completada", Id = id });

                    if (result > 0)
                    {
                        return Ok("Cita completada exitosamente.");
                    }
                    else
                    {
                        return NotFound("Cita no encontrada.");
                    }
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


        //Parte Administrativa 
    }
}

