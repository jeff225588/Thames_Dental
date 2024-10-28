using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Thames_Dental_API.Models;

namespace Thames_Dental_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecetarioController : ControllerBase
    {
        private readonly string _connectionString;

        public RecetarioController(IConfiguration configuration)
        {
            // Inicializar _connectionString con la cadena de conexión del archivo appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException(nameof(configuration), "La cadena de conexión no puede ser nula");
        }

        [HttpPost("AgregarReceta")]
        public async Task<IActionResult> AgregarReceta([FromBody] Recetario recetario)
        {
            // Validar que los datos de la nota no sean nulos
            if (recetario == null)
            {
                return BadRequest("Los datos de la receta no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Paciente", recetario.Paciente);
                parameters.Add("@Medicamento", recetario.Medicamento);
                parameters.Add("@Instrucciones", recetario.Instrucciones); 

                // Llamar al procedimiento almacenado utilizando Dapper
                await connection.ExecuteAsync("sp_InsertarPacienteMedicamento", parameters, commandType: CommandType.StoredProcedure);
            }

            return Ok("Receta agregada exitosamente.");
        }



    }
}
