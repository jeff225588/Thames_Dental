using Dapper;
using InventarioAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;


namespace InventarioAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {

        private readonly string _connectionString;

        public InventarioController(IConfiguration configuration)
        {
            // Inicializar _connectionString con la cadena de conexión del archivo appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException(nameof(configuration), "La cadena de conexión no puede ser nula");
        }



        [HttpPost("AgregarInventario")]
        public async Task<IActionResult> AgregarInventario([FromBody] Inventario inventario)
        {
            // Validar que los datos del cliente no sean nulos 
            if (inventario == null)
            {
                return BadRequest("Los datos del inventario no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Nombre", inventario.Nombre);
                parameters.Add("@Descripcion", inventario.Descripcion);
                parameters.Add("@Cantidad", inventario.Cantidad);
                parameters.Add("@Proveedor", inventario.Proveedor);
                parameters.Add("@PrecioUnitario", inventario.PrecioUnitario);
                parameters.Add("@FechaIngreso", inventario.FechaIngreso);

                // Llamar al procedimiento almacenado utilizando Dapper
                await connection.ExecuteAsync("sp_AgregarInventario", parameters, commandType: CommandType.StoredProcedure);
            }

            return Ok("Inventario agregado exitosamente.");
        }


        [HttpGet("ListarInventario")]
        public async Task<IActionResult> ListarInventario()
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                var inventario = await connection.QueryAsync<Inventario>("ListarInventario1", commandType: CommandType.StoredProcedure);

                return Ok(inventario);
            }
        }



        //[HttpDelete("EliminarInventario/{IdInventario}")]
        //public async Task<IActionResult> EliminarInventario(int IdInventario)
        //{
        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@IdInventario", IdInventario);

        //        var result = await connection.ExecuteAsync("sp_DeleteInventario", parameters, commandType: CommandType.StoredProcedure);

        //        if (result > 0)
        //        {
        //            return Ok(new { message = "Inventario eliminado exitosamente." });
        //        }
        //        else
        //        {
        //            return NotFound(new { message = "Inventario no encontrado." });
        //        }
        //    }
        //}


        [HttpDelete("EliminarInventario/{IdInventario}")]
        public async Task<IActionResult> EliminarInventario(int IdInventario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IdInventario", IdInventario);

                var result = await connection.ExecuteAsync("sp_DesactivarInventario", parameters, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    return Ok(new { message = "Inventario eliminado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "Inventario no encontrado." });
                }
            }
        }



        [HttpPost("RestaurarInventario/{IdInventario}")]
        public async Task<IActionResult> RestaurarInventario(int IdInventario)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IdInventario", IdInventario);

                var result = await connection.ExecuteAsync("sp_RestaurarInventario", parameters, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    return Ok(new { message = "Inventario restaurado exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "Inventario no encontrado o ya está activo." });
                }
            }
        }



        [HttpGet("ListarInventarioInactivo")]
        public async Task<IActionResult> ListarInventarioInactivo()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "sp_ListarInventarioInactivo";
                var result = await connection.QueryAsync<Inventario>(query, commandType: CommandType.StoredProcedure);

                if (result != null && result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(new { message = "No hay inventarios inactivos." });
                }
            }
        }





        [HttpPost("EditarInventario")]
        public async Task<IActionResult> EditarInventario([FromBody] Inventario inventarioRequest)
        {
            if (inventarioRequest == null)
            {
                return BadRequest("Los datos del inventario no pueden ser nulos.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IdInventario", inventarioRequest.IdInventario);
                parameters.Add("@Nombre", inventarioRequest.Nombre);
                parameters.Add("@Descripcion", inventarioRequest.Descripcion);
                parameters.Add("@Cantidad", inventarioRequest.Cantidad);
                parameters.Add("@Proveedor", inventarioRequest.Proveedor);
                parameters.Add("@PrecioUnitario", inventarioRequest.PrecioUnitario);
                parameters.Add("@FechaIngreso", inventarioRequest.FechaIngreso);
                try
                {
                    await connection.ExecuteAsync("EditarInventario", parameters, commandType: CommandType.StoredProcedure);
                    return Ok("Inventario editado exitosamente.");
                }
                catch (SqlException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }





        [HttpGet("ObtenerInventario/{IdInventario}")]
        public async Task<IActionResult> ObtenerInventario(int IdInventario)
        {
            if (IdInventario <= 0)
            {
                return BadRequest("El IdInventario es inválido.");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@IdInventario", IdInventario);

                    // Ejecutamos el procedimiento almacenado
                    var inventario = await connection.QueryFirstOrDefaultAsync<Inventario>(
                        "ObtenerInventario", parameters, commandType: CommandType.StoredProcedure
                    );

                    if (inventario == null)
                    {
                        return NotFound("Inventario no encontrado.");
                    }

                    return Ok(inventario);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }



        [HttpGet("GenerarReportePDF")]
        public async Task<IActionResult> GenerarReportePDF()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Obtener los datos del inventario desde la base de datos
                var inventario = await connection.QueryAsync<Inventario>("ListarInventario", commandType: CommandType.StoredProcedure);

                // Validar que exista al menos un registro
                if (inventario == null || !inventario.Any())
                {
                    return NotFound("No se encontraron datos en el inventario.");
                }

                // Crear un nuevo documento PDF
                PdfDocument documento = new PdfDocument();
                documento.Info.Title = "Reporte de Inventario";

                // Crear una página en el documento
                PdfPage pagina = documento.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                // Estilos de texto
                XFont fuenteTitulo = new XFont("Times New Roman", 22, XFontStyle.Bold);
                gfx.DrawString("Reporte de Inventario", fuenteTitulo, XBrushes.Black,
                               new XRect(0, 40, pagina.Width, 40), XStringFormats.TopCenter);

                // Texto adicional (opcional)
                XFont fuenteInfo = new XFont("Verdana", 12, XFontStyle.Regular);
                gfx.DrawString("Generado por el sistema de gestión de inventario Thames Dental.", fuenteInfo, XBrushes.Gray,
                               new XRect(0, 70, pagina.Width, 40), XStringFormats.TopCenter);

                // Dibujar encabezados de columna
                XFont fuenteTexto = new XFont("Times New Roman", 12, XFontStyle.Regular);
                int posY = 120;

                gfx.DrawString("ID", fuenteTexto, XBrushes.Black, new XPoint(30, posY));
                gfx.DrawString("Nombre", fuenteTexto, XBrushes.Black, new XPoint(70, posY));
                gfx.DrawString("Cantidad", fuenteTexto, XBrushes.Black, new XPoint(250, posY));
                gfx.DrawString("Proveedor", fuenteTexto, XBrushes.Black, new XPoint(350, posY));
                gfx.DrawString("Precio Unitario", fuenteTexto, XBrushes.Black, new XPoint(470, posY));
                gfx.DrawString("Fecha Ingreso", fuenteTexto, XBrushes.Black, new XPoint(600, posY));

                posY += 20;

                // Agregar datos del inventario al PDF
                foreach (var item in inventario)
                {
                    string nombreTruncado = item.Nombre.Length > 25 ? item.Nombre.Substring(0, 22) + "..." : item.Nombre;

                    gfx.DrawString(item.IdInventario.ToString(), fuenteTexto, XBrushes.Black, new XPoint(30, posY));
                    gfx.DrawString(nombreTruncado, fuenteTexto, XBrushes.Black, new XPoint(70, posY));
                    gfx.DrawString(item.Cantidad.ToString(), fuenteTexto, XBrushes.Black, new XPoint(250, posY));
                    gfx.DrawString(item.Proveedor, fuenteTexto, XBrushes.Black, new XPoint(350, posY));
                    gfx.DrawString(item.PrecioUnitario.ToString("C", new System.Globalization.CultureInfo("es-CR")), fuenteTexto, XBrushes.Black, new XPoint(470, posY));
                    gfx.DrawString(item.FechaIngreso.ToShortDateString(), fuenteTexto, XBrushes.Black, new XPoint(600, posY));

                    posY += 20;

                    // Crear una nueva página si la posición Y supera el límite
                    if (posY > pagina.Height - 40)
                    {
                        pagina = documento.AddPage();
                        gfx = XGraphics.FromPdfPage(pagina);
                        posY = 40;

                        // Redibujar encabezados en la nueva página
                        gfx.DrawString("ID", fuenteTexto, XBrushes.Black, new XPoint(30, posY));
                        gfx.DrawString("Nombre", fuenteTexto, XBrushes.Black, new XPoint(70, posY));
                        gfx.DrawString("Cantidad", fuenteTexto, XBrushes.Black, new XPoint(250, posY));
                        gfx.DrawString("Proveedor", fuenteTexto, XBrushes.Black, new XPoint(350, posY));
                        gfx.DrawString("Precio Unitario", fuenteTexto, XBrushes.Black, new XPoint(470, posY));
                        gfx.DrawString("Fecha Ingreso", fuenteTexto, XBrushes.Black, new XPoint(600, posY));
                        posY += 20;
                    }
                }

                // Guardar el documento PDF en un MemoryStream
                using (MemoryStream stream = new MemoryStream())
                {
                    documento.Save(stream, false);

                    // Devolver el archivo como un archivo descargable
                    return File(stream.ToArray(), "application/pdf", "ReporteInventario.pdf");
                }
            }
        }














    }
}
