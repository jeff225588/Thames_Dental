using Thames_Dental_Web.Models;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Thames_Dental_Web.Filters;


namespace Inventario.Controllers
{
    [AdminFilter]
    public class InventarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _conf;

        public InventarioController(IHttpClientFactory http, IConfiguration conf)
        {
            _http = http;
            _conf = conf;
        }


        [HttpGet]
        public IActionResult Inventario()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Inventario(InventarioModel model)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/AgregarInventario";


                JsonContent datos = JsonContent.Create(model);

                var response = client.PostAsync(url, datos).Result;


                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Inventario agregado exitosamente.";
                }
                else
                {
                    ViewBag.Message = "Error al guardar el inventario.";
                }

                return View(); // Recargas la misma vista
            }


        }


        [HttpGet]
        public async Task<IActionResult> ListarInventario()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/ListarInventario";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var inventario = await response.Content.ReadFromJsonAsync<List<InventarioModel>>();
                    return View("ListarInventario", inventario); // Cargar la nueva vista con los datos
                }
                else
                {
                    ViewBag.Message = "Error al obtener la lista de inventario.";
                    return View("ListarInventario", new List<InventarioModel>()); // Retornar una lista vacía en caso de error
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> ListarAuditoriaInventario()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/ListarAuditoriaInventario";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var inventario = await response.Content.ReadFromJsonAsync<List<InventarioModel>>();
                    return View("ListarAuditoriaInventario", inventario); // Cargar la nueva vista con los datos
                }
                else
                {
                    ViewBag.Message = "Error al obtener la lista de inventario.";
                    return View("ListarAuditoriaInventario", new List<InventarioModel>()); // Retornar una lista vacía en caso de error
                }
            }
        }


        //[HttpPost("Inventario/EliminarInventario/{IdInventario}")]
        //public async Task<IActionResult> EliminarInventario(int IdInventario)
        //{
        //    using (var client = _http.CreateClient())
        //    {
        //        // URL que apunta a tu API
        //        string url = _conf.GetSection("Variables:RutaApi").Value + $"Inventario/EliminarInventario/{IdInventario}";
        //        var response = await client.DeleteAsync(url);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            TempData["SuccessMessage"] = "Inventario eliminado exitosamente.";
        //        }
        //        else
        //        {
        //            TempData["ErrorMessage"] = "Error al eliminar el Inventario.";
        //        }

        //        // Redirige nuevamente a la lista de inventario después de eliminar
        //        return RedirectToAction("ListarInventario");
        //    }
        //}


        [HttpPost("Inventario/EliminarInventario/{IdInventario}")]
        public async Task<IActionResult> EliminarInventario(int IdInventario)
        {
            using (var client = _http.CreateClient())
            {
                // URL que apunta a tu API
                string url = _conf.GetSection("Variables:RutaApi").Value + $"Inventario/EliminarInventario/{IdInventario}";
                var response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Inventario eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al eliminar el Inventario.";
                }

                // Redirige nuevamente a la lista de inventario después de eliminar
                return RedirectToAction("ListarInventario");
            }
        }




        [HttpPost("Inventario/RestaurarInventario/{IdInventario}")]
        public async Task<IActionResult> RestaurarInventario(int IdInventario)
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + $"Inventario/RestaurarInventario/{IdInventario}";
                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Inventario restaurado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al restaurar el Inventario.";
                }

                return RedirectToAction("InventarioInactivo");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ListarInventarioInactivo()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/ListarInventarioInactivo";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var inventario = await response.Content.ReadFromJsonAsync<List<InventarioModel>>();
                    return View("ListarInventarioInactivo", inventario); // Cargar la nueva vista con los datos
                }
                else
                {
                    ViewBag.Message = "Error al obtener la lista de inventario.";
                    return View("ListarInventarioInactivo", new List<InventarioModel>()); // Retornar una lista vacía en caso de error
                }
            }
        }






        [HttpGet]
        public async Task<IActionResult> EditarInventario(int IdInventario)
        {
            InventarioModel? model = null;  // Usar el operador de tipo nullable "?"

            try
            {
                using (var client = _http.CreateClient())
                {
                    string url = _conf.GetSection("Variables:RutaApi").Value + $"Inventario/ObtenerInventario/{IdInventario}";
                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        model = await response.Content.ReadFromJsonAsync<InventarioModel>();
                    }
                    else
                    {
                        ViewBag.Message = "Error al cargar los datos del inventario.";
                        return View("Error");
                    }
                }

                if (model == null)  // Si model es nulo, maneja el caso
                {
                    ViewBag.Message = "Inventario no encontrado.";
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
        public IActionResult EditarInventario(InventarioModel model)
        {
            using (var client = _http.CreateClient())
            {
                // Construir la URL para llamar al método EditarInventario en el API
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/EditarInventario";

                // Crear el contenido JSON a partir del modelo para enviarlo en el cuerpo de la solicitud
                JsonContent datos = JsonContent.Create(model);

                // Realizar una solicitud POST a la URL especificada con el contenido JSON
                var response = client.PostAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "Inventario actualizado exitosamente.";
                }
                else
                {
                    ViewBag.Message = "Error al actualizar el inventario.";
                }

                return RedirectToAction("ListarInventario"); // Redirigir a la vista de lista de expedientes
            }
        }



        [HttpGet]
        public async Task<IActionResult> GenerarReporteInventario()
        {
            using (var client = _http.CreateClient())
            {
                string url = _conf.GetSection("Variables:RutaApi").Value + "Inventario/ListarInventario";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var inventario = await response.Content.ReadFromJsonAsync<List<InventarioModel>>();

                    PdfDocument documento = new PdfDocument();
                    documento.Info.Title = "Reporte de Inventario";

                    PdfPage pagina = documento.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(pagina);


                    XFont fuenteTitulo = new XFont("Times New Roman", 22, XFontStyle.Bold);
                    gfx.DrawString("Reporte de Inventario", fuenteTitulo, XBrushes.Black, new XRect(0, 80, pagina.Width, 40), XStringFormats.TopCenter);

                    XFont fuenteTexto = new XFont("Times New Roman", 12, XFontStyle.Regular);
                    int posY = 120;

                    gfx.DrawString("ID", fuenteTexto, XBrushes.Black, new XPoint(30, posY));
                    gfx.DrawString("Nombre", fuenteTexto, XBrushes.Black, new XPoint(70, posY));
                    gfx.DrawString("Cantidad", fuenteTexto, XBrushes.Black, new XPoint(250, posY));
                    gfx.DrawString("Proveedor", fuenteTexto, XBrushes.Black, new XPoint(350, posY));
                    gfx.DrawString("Precio Unitario", fuenteTexto, XBrushes.Black, new XPoint(470, posY));
                    gfx.DrawString("Fecha Ingreso", fuenteTexto, XBrushes.Black, new XPoint(600, posY));

                    posY += 20;

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

                        if (posY > pagina.Height - 40)
                        {
                            pagina = documento.AddPage();
                            gfx = XGraphics.FromPdfPage(pagina);
                            posY = 40;

                            gfx.DrawString("ID", fuenteTexto, XBrushes.Black, new XPoint(30, posY));
                            gfx.DrawString("Nombre", fuenteTexto, XBrushes.Black, new XPoint(70, posY));
                            gfx.DrawString("Cantidad", fuenteTexto, XBrushes.Black, new XPoint(250, posY));
                            gfx.DrawString("Proveedor", fuenteTexto, XBrushes.Black, new XPoint(350, posY));
                            gfx.DrawString("Precio Unitario", fuenteTexto, XBrushes.Black, new XPoint(470, posY));
                            gfx.DrawString("Fecha Ingreso", fuenteTexto, XBrushes.Black, new XPoint(600, posY));
                            posY += 20;
                        }
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        documento.Save(stream, false);
                        return File(stream.ToArray(), "application/pdf", "ReporteInventario.pdf");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ViewBag.Message = $"Error al generar el reporte: {error}";
                    return RedirectToAction("ListarInventario");
                }
            }
        }














    }
}
