using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Thames_Dental_API.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

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
        [HttpGet("ObtenerCitas")]
        public async Task<IActionResult> ObtenerCitas()
        {
            var connectionString = _conf.GetSection("ConnectionStrings:DefaultConnection").Value;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var citas = await connection.QueryAsync<Cita>("SELECT * FROM Citas");
                    return Ok(citas);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener citas: {ex.Message}");
                return StatusCode(500, "Error al obtener citas.");
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
                    var query = "UPDATE Citas SET Estado = @Estado WHERE Id = @Id";
                    var affectedRows = await connection.ExecuteAsync(query, new { Estado = "Cancelada", Id = id });

                    Console.WriteLine($"Filas afectadas al cancelar la cita: {affectedRows}");

                    if (affectedRows > 0)
                    {
                        return Ok(); // Simplemente regresa 200 OK
                    }
                    else
                    {
                        return NotFound(); // 404 si no se encontró la cita
                    }
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


        //Parte Administrativa 
    }
}

