using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Thames_Dental_API.Models;
using System.Threading.Tasks;

namespace Thames_Dental_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpedienteController : ControllerBase
    {
        private readonly string _connectionString;

        public ExpedienteController(IConfiguration configuration)
        {
            // Inicializar _connectionString con la cadena de conexión del archivo appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException(nameof(configuration), "La cadena de conexión no puede ser nula");
        }

        [HttpPost("AgregarCliente")]
        public async Task<IActionResult> AgregarCliente([FromBody] Cliente cliente)
        {
            // Validar que los datos del cliente no sean nulos 
            if (cliente == null)
            {
                return BadRequest("Los datos del cliente no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Nombre", cliente.Nombre);
                parameters.Add("@PrimerApellido", cliente.PrimerApellido);
                parameters.Add("@SegundoApellido", cliente.SegundoApellido);
                parameters.Add("@TipoIdentificacion", cliente.TipoIdentificacion);
                parameters.Add("@Identificacion", cliente.Identificacion);
                parameters.Add("@TelefonoPrincipal", cliente.TelefonoPrincipal);
                parameters.Add("@CorreoElectronico", cliente.CorreoElectronico);
                parameters.Add("@Pais", cliente.Pais);
                parameters.Add("@Ciudad", cliente.Ciudad);
                parameters.Add("@DireccionExacta", cliente.DireccionExacta);
                parameters.Add("@FechaNacimiento", cliente.FechaNacimiento);
                parameters.Add("@Genero", cliente.Genero);
                parameters.Add("@Ocupacion", cliente.Ocupacion);
                parameters.Add("@ContactoEmergencia", cliente.ContactoEmergencia);
                parameters.Add("@TelefonoEmergencia", cliente.TelefonoEmergencia);
                parameters.Add("@FechaIngreso", cliente.FechaIngreso);
                parameters.Add("@Notas", cliente.Notas);

                // Llamar al procedimiento almacenado utilizando Dapper
                await connection.ExecuteAsync("sp_AgregarCliente", parameters, commandType: CommandType.StoredProcedure);
            }

            return Ok("Cliente agregado exitosamente.");
        }



        [HttpPost("AgregarNota")]
        public async Task<IActionResult> AgregarNota([FromBody] Nota nota)
        {
            // Validar que los datos de la nota no sean nulos
            if (nota == null)
            {
                return BadRequest("Los datos de la nota no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Fecha", nota.Fecha);
                parameters.Add("@Detalle", nota.Detalle);
                parameters.Add("@Cedula", nota.Cedula); // Se pasa la cédula

                // Llamar al procedimiento almacenado utilizando Dapper
                await connection.ExecuteAsync("sp_AgregarNota", parameters, commandType: CommandType.StoredProcedure);
            }

            return Ok("Nota agregada exitosamente.");
        }


        [HttpPost("verNota")]
        public async Task<IActionResult> verNota([FromBody] Nota nota)
        {
            // Validar que la cédula no sea nula o vacía
            if (string.IsNullOrEmpty(nota.Cedula))
            {
                return BadRequest("La cédula no puede ser nula o vacía.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", nota.Cedula); // Se pasa solo la cédula

                // Llamar al procedimiento almacenado utilizando Dapper para obtener resultados
                var notas = await connection.QueryAsync<Nota>("sp_VerNotasPorCedula", parameters, commandType: CommandType.StoredProcedure);

                // Verificar si se encontraron notas
                if (notas == null || !notas.Any())
                {
                    return NotFound("No se encontraron notas para la cédula proporcionada.");
                }

                return Ok(notas); // Devolver la lista de notas
            }
        }

        [HttpPost("AgregarAutorizado")]
        public async Task<IActionResult> AgregarAutorizado([FromBody] Autorizado autorizado)
        {
            if (autorizado == null)
            {
                return BadRequest("Los datos del autorizado no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", autorizado.Cedula);
                parameters.Add("@Nombre", autorizado.Nombre);
                parameters.Add("@TelefonoEmergencia", autorizado.TelefonoEmergencia);

                try
                {
                    await connection.ExecuteAsync("sp_AgregarAutorizados", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Ok("Autorizado agregado exitosamente.");
        }

        [HttpGet("ListarClientes")]
        public async Task<IActionResult> ListarClientes()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                
                var clientes = await connection.QueryAsync<Cliente>("ListarExpedientesCliente", commandType: CommandType.StoredProcedure);

                return Ok(clientes);
            }
        }







    }





}
    
