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
                parameters.Add("@Estado", cliente.Estado);

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


        [HttpPost("AgregarReceta")]
        public async Task<IActionResult> AgregarReceta([FromBody] RecetaRequest recetaRequest)
        {
            if (recetaRequest == null)
            {
                return BadRequest("Los datos de la receta no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Identificacion", recetaRequest.Identificacion);
                parameters.Add("@Medicamento", recetaRequest.Medicamento);
                parameters.Add("@Instrucciones", recetaRequest.Instrucciones);

                try
                {
                    await connection.ExecuteAsync("sp_InsertarReceta", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Ok("Receta agregada exitosamente.");
        }

        [HttpPost("AgregarHistorialClinico")]
        public async Task<IActionResult> AgregarHistorialClinico([FromBody] HistorialClinicoRequest historialClinicoRequest)
        {
            if (historialClinicoRequest == null)
            {
                return BadRequest("Los datos del historial clínico no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();

                // Parámetros básicos
                parameters.Add("@CedulaPaciente", historialClinicoRequest.CedulaPaciente);
                parameters.Add("@TipoSangre", historialClinicoRequest.TipoSangre);
                parameters.Add("@Anotaciones", historialClinicoRequest.Anotaciones);
                parameters.Add("@DolorMolestia", historialClinicoRequest.DolorMolestia);
                parameters.Add("@Nervioso", historialClinicoRequest.Nervioso);
                parameters.Add("@AtencionMedica", historialClinicoRequest.AtencionMedica);
                parameters.Add("@Medicamentos", historialClinicoRequest.Medicamentos);
                parameters.Add("@Alergico", historialClinicoRequest.Alergico);
                parameters.Add("@SangradoExcesivo", historialClinicoRequest.SangradoExcesivo);
                parameters.Add("@ReaccionAnestesica", historialClinicoRequest.ReaccionAnestesica);

                // Condiciones médicas
                parameters.Add("@FallaCardiaca", historialClinicoRequest.FallaCardiaca);
                parameters.Add("@Infarto", historialClinicoRequest.Infarto);
                parameters.Add("@AnginaPecho", historialClinicoRequest.AnginaPecho);
                parameters.Add("@PresionAlta", historialClinicoRequest.PresionAlta);
                parameters.Add("@SoploCorazon", historialClinicoRequest.SoploCorazon);
                parameters.Add("@LesionesCardiacasCongenitas", historialClinicoRequest.LesionesCardiacasCongenitas);
                parameters.Add("@ValvulasCardiacasArtificiales", historialClinicoRequest.ValvulasCardiacasArtificiales);
                parameters.Add("@Marcapasos", historialClinicoRequest.Marcapasos);
                parameters.Add("@OperacionCorazon", historialClinicoRequest.OperacionCorazon);
                parameters.Add("@Transplante", historialClinicoRequest.Transplante);
                parameters.Add("@HerpesLabial", historialClinicoRequest.HerpesLabial);
                parameters.Add("@Anemia", historialClinicoRequest.Anemia);
                parameters.Add("@DerrameCerebral", historialClinicoRequest.DerrameCerebral);
                parameters.Add("@ProblemasRinon", historialClinicoRequest.ProblemasRinon);
                parameters.Add("@UlcerasGastritis", historialClinicoRequest.UlcerasGastritis);
                parameters.Add("@EnfisemaPulmonar", historialClinicoRequest.EnfisemaPulmonar);
                parameters.Add("@TosFrecuente", historialClinicoRequest.TosFrecuente);
                parameters.Add("@VIHPositivo", historialClinicoRequest.VIHPositivo);
                parameters.Add("@EnfermedadVenera", historialClinicoRequest.EnfermedadVenera);
                parameters.Add("@Asma", historialClinicoRequest.Asma);
                parameters.Add("@Tuberculosis", historialClinicoRequest.Tuberculosis);
                parameters.Add("@FiebreReumatica", historialClinicoRequest.FiebreReumatica);
                parameters.Add("@Sinusitis", historialClinicoRequest.Sinusitis);
                parameters.Add("@Alergias", historialClinicoRequest.Alergias);
                parameters.Add("@Diabetes", historialClinicoRequest.Diabetes);
                parameters.Add("@ProblemasTiroides", historialClinicoRequest.ProblemasTiroides);
                parameters.Add("@Radioterapia", historialClinicoRequest.Radioterapia);
                parameters.Add("@Quimioterapia", historialClinicoRequest.Quimioterapia);
                parameters.Add("@Artritis", historialClinicoRequest.Artritis);
                parameters.Add("@Reumatismo", historialClinicoRequest.Reumatismo);
                parameters.Add("@TratamientoPsiquiatrico", historialClinicoRequest.TratamientoPsiquiatrico);
                parameters.Add("@DolorUnionMandibular", historialClinicoRequest.DolorUnionMandibular);
                parameters.Add("@HepatitisA", historialClinicoRequest.HepatitisA);
                parameters.Add("@HepatitisB", historialClinicoRequest.HepatitisB);
                parameters.Add("@HepatitisC", historialClinicoRequest.HepatitisC);
                parameters.Add("@ProblemasHepaticos", historialClinicoRequest.ProblemasHepaticos);
                parameters.Add("@TransfusionSangre", historialClinicoRequest.TransfusionSangre);
                parameters.Add("@EpilepsiaConvulsiones", historialClinicoRequest.EpilepsiaConvulsiones);
                parameters.Add("@Desmayos", historialClinicoRequest.Desmayos);
                parameters.Add("@Nerviosismo", historialClinicoRequest.Nerviosismo);
                parameters.Add("@AparicionHematomas", historialClinicoRequest.AparicionHematomas);
                parameters.Add("@OtraCondicion", historialClinicoRequest.OtraCondicion);

                // Otros parámetros
                parameters.Add("@MedicamentosUltimos2Meses", historialClinicoRequest.MedicamentosUltimos2Meses);
                parameters.Add("@Operaciones", historialClinicoRequest.Operaciones);

                try
                {
                    // Ejecutar procedimiento almacenado
                    await connection.ExecuteAsync("InsertarHistorialClinico", parameters, commandType: CommandType.StoredProcedure);
                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return Ok("Historial clínico agregado exitosamente.");
        }


        [HttpDelete("EliminarExpediente/{id}")]
        public async Task<IActionResult> EliminarExpediente(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ClienteID", id);

                var result = await connection.ExecuteAsync("sp_DeleteExpedienteCliente", parameters, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    return Ok(new { message = "Expediente eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "Expediente no encontrado." });
                }
            }
        }





        [HttpPost("EditarExpediente")]
        public async Task<IActionResult> EditarExpediente([FromBody] Cliente expedienteRequest)
        {
            if (expedienteRequest == null)
            {
                return BadRequest("Los datos del expediente no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ClienteID", expedienteRequest.ClienteID);
                parameters.Add("@Nombre", expedienteRequest.Nombre);
                parameters.Add("@PrimerApellido", expedienteRequest.PrimerApellido);
                parameters.Add("@SegundoApellido", expedienteRequest.SegundoApellido);
                parameters.Add("@TipoIdentificacion", expedienteRequest.TipoIdentificacion);
                parameters.Add("@Identificacion", expedienteRequest.Identificacion);
                parameters.Add("@TelefonoPrincipal", expedienteRequest.TelefonoPrincipal);
                parameters.Add("@CorreoElectronico", expedienteRequest.CorreoElectronico);
                parameters.Add("@Pais", expedienteRequest.Pais);
                parameters.Add("@Ciudad", expedienteRequest.Ciudad);
                parameters.Add("@DireccionExacta", expedienteRequest.DireccionExacta);
                parameters.Add("@FechaNacimiento", expedienteRequest.FechaNacimiento);
                parameters.Add("@Genero", expedienteRequest.Genero);
                parameters.Add("@Ocupacion", expedienteRequest.Ocupacion);
                parameters.Add("@ContactoEmergencia", expedienteRequest.ContactoEmergencia);
                parameters.Add("@TelefonoEmergencia", expedienteRequest.TelefonoEmergencia);
                parameters.Add("@FechaIngreso", expedienteRequest.FechaIngreso);
                parameters.Add("@Notas", expedienteRequest.Notas);

                try
                {
                    await connection.ExecuteAsync("EditarExpediente", parameters, commandType: CommandType.StoredProcedure);
                    return Ok("Expediente editado exitosamente.");
                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }


        [HttpGet("ObtenerCliente/{clienteID}")]
        public async Task<IActionResult> ObtenerCliente(int clienteID)
        {
            if (clienteID <= 0)
            {
                return BadRequest("El ClienteID es inválido.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteID);

                    // Ejecutamos el procedimiento almacenado
                    var cliente = await connection.QueryFirstOrDefaultAsync<Cliente>(
                        "ObtenerCliente", parameters, commandType: CommandType.StoredProcedure
                    );

                    if (cliente == null)
                    {
                        return NotFound("Cliente no encontrado.");
                    }

                    return Ok(cliente);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet("ObtenerRecetasPorCliente/{clienteID}")]
        public async Task<IActionResult> ObtenerRecetasPorCliente(int clienteID)
        {
            if (clienteID <= 0)
            {
                return BadRequest("El ClienteID es inválido.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteID);

                    // Ejecutamos el procedimiento almacenado
                    var recetas = await connection.QueryAsync<RecetaRequest>(
                        "BuscarRecetaPorIdCliente", parameters, commandType: CommandType.StoredProcedure
                    );

                    // Verificar si no se encuentran recetas
                    if (recetas == null || !recetas.Any() ||
                        (recetas.First().Id == 0 && string.IsNullOrEmpty(recetas.First().Medicamento)))
                    {
                        return NotFound("El cliente no tiene recetas.");
                    }

                    return Ok(recetas);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        [HttpPost("AgregarRecetaporid")]
        public async Task<IActionResult> AgregarRecetaporid([FromBody] RecetaRequest recetaRequest)
        {
            if (recetaRequest == null || recetaRequest.ClienteID <= 0 || string.IsNullOrEmpty(recetaRequest.Medicamento))
            {
                return BadRequest("Los datos de la receta son inválidos.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", recetaRequest.ClienteID);
                    parameters.Add("@Medicamento", recetaRequest.Medicamento);
                    parameters.Add("@Instrucciones", recetaRequest.Instrucciones);

                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QuerySingleOrDefaultAsync<string>(
                        "AgregarRecetaporid", parameters, commandType: CommandType.StoredProcedure
                    );

                    // Verificar el mensaje de resultado
                    if (result.Contains("éxito"))
                    {
                        return Ok(result); // Mensaje de éxito
                    }
                    else
                    {
                        return BadRequest(result); // Mensaje de error
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }



        [HttpGet("ObtenerAutorizadosPorClienteID/{clienteId}")]
        public async Task<IActionResult> ObtenerAutorizadosPorClienteID(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest(new { success = false, message = "El ClienteID no es válido." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteId);

                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QueryAsync<Autorizado>(
                        "ObtenerAutorizadosPorClienteID", parameters, commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Any())
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "No se encontraron autorizados para el ClienteID proporcionado." });
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener los autorizados: {ex.Message}" });
            }
        }



        [HttpPost("AgregarAutorizadoporid")]
        public async Task<IActionResult> AgregarAutorizadoporid([FromBody] Autorizado autorizadoRequest)
        {
            if (autorizadoRequest == null || autorizadoRequest.ClienteID <= 0 || string.IsNullOrEmpty(autorizadoRequest.Nombre))
            {
                return BadRequest("Los datos del autorizado son inválidos.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", autorizadoRequest.ClienteID);
                    parameters.Add("@Nombre", autorizadoRequest.Nombre);
                    parameters.Add("@TelefonoEmergencia", autorizadoRequest.TelefonoEmergencia);

                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QuerySingleOrDefaultAsync<string>(
                        "sp_AgregarAutorizadoporid", parameters, commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Contains("correctamente"))
                    {
                        return Ok(result); // Mensaje de éxito
                    }
                    else
                    {
                        return BadRequest("Error al agregar el autorizado: " + result); // Mensaje de error
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}"); // Error de servidor interno
            }
        }



        [HttpGet("ObtenerHistorialClinicoPorClienteID/{clienteId}")]
        public async Task<IActionResult> ObtenerHistorialClinicoPorClienteID(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest(new { success = false, message = "El ClienteID no es válido." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteId);

                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QueryAsync<HistorialClinicoporid>(
                        "sp_ObtenerHistorialClinicoPorClienteID", parameters, commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Any())
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return NotFound(new { success = false, message = "No se encontraron registros de historial clínico para el ClienteID proporcionado." });
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener el historial clínico: {ex.Message}" });
            }
        }

        [HttpPost("AgregarHistorialClinicoPorId")]
        public async Task<IActionResult> AgregarHistorialClinicoPorId([FromBody] HistorialClinicoRequest historialRequest)
        {
            // Validación inicial
            if (historialRequest == null)
            {
                return BadRequest("El cuerpo de la solicitud está vacío.");
            }

            if (historialRequest.ClienteID <= 0)
            {
                return BadRequest("El ClienteID debe ser mayor que 0.");
            }

            try
            {
                // Establece la conexión a la base de datos
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Parámetros del procedimiento almacenado
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", historialRequest.ClienteID);
                    parameters.Add("@TipoSangre", historialRequest.TipoSangre);
                    parameters.Add("@Anotaciones", historialRequest.Anotaciones);
                    parameters.Add("@DolorMolestia", historialRequest.DolorMolestia);
                    parameters.Add("@Nervioso", historialRequest.Nervioso);
                    parameters.Add("@AtencionMedica", historialRequest.AtencionMedica);
                    parameters.Add("@Medicamentos", historialRequest.Medicamentos);
                    parameters.Add("@Alergico", historialRequest.Alergico);
                    parameters.Add("@SangradoExcesivo", historialRequest.SangradoExcesivo);
                    parameters.Add("@ReaccionAnestesica", historialRequest.ReaccionAnestesica);
                    parameters.Add("@MedicamentosUltimos2Meses", historialRequest.MedicamentosUltimos2Meses);
                    parameters.Add("@Operaciones", historialRequest.Operaciones);
                    parameters.Add("@FallaCardiaca", historialRequest.FallaCardiaca);
                    parameters.Add("@Infarto", historialRequest.Infarto);
                    parameters.Add("@AnginaPecho", historialRequest.AnginaPecho);
                    parameters.Add("@PresionAlta", historialRequest.PresionAlta);
                    parameters.Add("@SoploCorazon", historialRequest.SoploCorazon);
                    parameters.Add("@LesionesCardiacasCongenitas", historialRequest.LesionesCardiacasCongenitas);
                    parameters.Add("@ValvulasCardiacasArtificiales", historialRequest.ValvulasCardiacasArtificiales);
                    parameters.Add("@Marcapasos", historialRequest.Marcapasos);
                    parameters.Add("@OperacionCorazon", historialRequest.OperacionCorazon);
                    parameters.Add("@Transplante", historialRequest.Transplante);
                    parameters.Add("@HerpesLabial", historialRequest.HerpesLabial);
                    parameters.Add("@Anemia", historialRequest.Anemia);
                    parameters.Add("@DerrameCerebral", historialRequest.DerrameCerebral);
                    parameters.Add("@ProblemasRinon", historialRequest.ProblemasRinon);
                    parameters.Add("@UlcerasGastritis", historialRequest.UlcerasGastritis);
                    parameters.Add("@EnfisemaPulmonar", historialRequest.EnfisemaPulmonar);
                    parameters.Add("@TosFrecuente", historialRequest.TosFrecuente);
                    parameters.Add("@VIHPositivo", historialRequest.VIHPositivo);
                    parameters.Add("@EnfermedadVenera", historialRequest.EnfermedadVenera);
                    parameters.Add("@Asma", historialRequest.Asma);
                    parameters.Add("@Tuberculosis", historialRequest.Tuberculosis);
                    parameters.Add("@FiebreReumatica", historialRequest.FiebreReumatica);
                    parameters.Add("@Sinusitis", historialRequest.Sinusitis);
                    parameters.Add("@Alergias", historialRequest.Alergias);
                    parameters.Add("@Diabetes", historialRequest.Diabetes);
                    parameters.Add("@ProblemasTiroides", historialRequest.ProblemasTiroides);
                    parameters.Add("@Radioterapia", historialRequest.Radioterapia);
                    parameters.Add("@Quimioterapia", historialRequest.Quimioterapia);
                    parameters.Add("@Artritis", historialRequest.Artritis);
                    parameters.Add("@Reumatismo", historialRequest.Reumatismo);
                    parameters.Add("@TratamientoPsiquiatrico", historialRequest.TratamientoPsiquiatrico);
                    parameters.Add("@DolorUnionMandibular", historialRequest.DolorUnionMandibular);
                    parameters.Add("@HepatitisA", historialRequest.HepatitisA);
                    parameters.Add("@HepatitisB", historialRequest.HepatitisB);
                    parameters.Add("@HepatitisC", historialRequest.HepatitisC);
                    parameters.Add("@ProblemasHepaticos", historialRequest.ProblemasHepaticos);
                    parameters.Add("@TransfusionSangre", historialRequest.TransfusionSangre);
                    parameters.Add("@EpilepsiaConvulsiones", historialRequest.EpilepsiaConvulsiones);
                    parameters.Add("@Desmayos", historialRequest.Desmayos);
                    parameters.Add("@Nerviosismo", historialRequest.Nerviosismo);
                    parameters.Add("@AparicionHematomas", historialRequest.AparicionHematomas);
                    parameters.Add("@OtraCondicion", historialRequest.OtraCondicion);

                    try
                    {
                        // Ejecutar procedimiento almacenado
                        await connection.ExecuteAsync("sp_AgregarHistorialClinicoporid", parameters, commandType: CommandType.StoredProcedure);
                    }
                    catch (SqlException ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }

                return Ok("Historial clínico agregado exitosamente.");


            }


            catch (Exception ex)
            {
                return BadRequest($"Ocurrió un error inesperado: {ex.Message}");
            }
        }


        [HttpGet("VerNotasPorID/{clienteId}")]
        public async Task<IActionResult> VerNotasPorID(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest("El ClienteID no es válido.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteId);

                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QueryAsync<Nota>(
                        "VerNotasPorID",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Any())
                    {
                        // Devolver directamente la lista de notas
                        return Ok(result);
                    }
                    else
                    {
                        // Devolver un mensaje claro en caso de no encontrar datos
                        return NotFound("No hay recetas asociadas a este cliente.");
                    }
                }
            }
            catch (SqlException ex)
            {
                // Devolver un error en caso de excepción
                return StatusCode(500, $"Error al obtener las recetas: {ex.Message}");
            }
        }



        [HttpPost("AgregarNotasPorID")]
        public async Task<IActionResult> AgregarNotasPorID([FromBody] Nota Nota)
        {
            if (Nota == null)
            {
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud está vacío." });
            }

            if (Nota.ClienteID <= 0 || string.IsNullOrEmpty(Nota.Detalle))
            {
                return BadRequest(new { success = false, message = "El ClienteID o Detalle no son válidos." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", Nota.ClienteID);
                    parameters.Add("@Fecha", Nota.Fecha);
                    parameters.Add("@Detalle", Nota.Detalle);

                    // Ejecutar el procedimiento almacenado
                    await connection.ExecuteAsync(
                        "AgregarNotasPorID",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new { success = true, message = "Receta agregada exitosamente." });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al agregar la receta: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error inesperado: {ex.Message}" });
            }
        }

        [HttpPost("ActualizarEstadoExpediente")]
        public async Task<IActionResult> ActualizarEstadoExpediente([FromBody] EstadoExpedienteRequest request)
        {
            if (request.ClienteID <= 0)
            {
                return BadRequest(new { success = false, message = "El ClienteID no es válido." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", request.ClienteID);
                    parameters.Add("@NuevoEstado", request.NuevoEstado);

                    // Ejecutar el procedimiento almacenado
                    await connection.ExecuteAsync(
                        "ActivarDesactivarExpediente",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return Ok(new { success = true, message = $"Estado del expediente actualizado a {(request.NuevoEstado ? "activo" : "inactivo")}." });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al actualizar el estado del expediente: {ex.Message}" });
            }
        }


        [HttpGet("ObtenerExpedientesDesactivados")]
        public async Task<IActionResult> ObtenerExpedientesDesactivados()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Ejecutar el procedimiento almacenado
                    var result = await connection.QueryAsync<Cliente>(
                        "MostrarExpedientesDesactivados",
                        commandType: CommandType.StoredProcedure
                    );

                    if (result != null && result.Any())
                    {
                        return Ok(result); // Solo devuelve la lista directamente
                    }
                    else
                    {
                        return NotFound(new { message = "No se encontraron expedientes desactivados." });
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { message = $"Error al obtener expedientes desactivados: {ex.Message}" });
            }
        }


        [HttpPost("InsertarCotizacion")]
        public async Task<IActionResult> InsertarCotizacion([FromBody] CotizacionModel cotizacion)
        {
            if (cotizacion == null || cotizacion.ClienteID <= 0 || string.IsNullOrEmpty(cotizacion.TextoCotizacion))
            {
                return BadRequest(new { success = false, message = "Datos de cotización inválidos." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", cotizacion.ClienteID);
                    parameters.Add("@TextoCotizacion", cotizacion.TextoCotizacion);

                    // Ejecutar el procedimiento almacenado para insertar la cotización
                    await connection.ExecuteAsync("InsertarCotizacionporid", parameters, commandType: CommandType.StoredProcedure);

                    return Ok(new { success = true, message = "Cotización insertada con éxito." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al insertar la cotización: {ex.Message}" });
            }
        }

        [HttpGet("ObtenerCotizacionesPorID/{clienteId}")]
        public async Task<IActionResult> ObtenerCotizacionesPorID(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest(new { success = false, message = "ClienteID no válido." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ClienteID", clienteId);

                    // Ejecutar el procedimiento almacenado para obtener las cotizaciones
                    var cotizaciones = await connection.QueryAsync<CotizacionModel>(
                        "ObtenerCotizacionesPorID",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Devolver solo los resultados (las cotizaciones) directamente sin la clave 'cotizaciones'
                    return Ok(cotizaciones);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error al obtener las cotizaciones: {ex.Message}" });
            }
        }



    }









}







