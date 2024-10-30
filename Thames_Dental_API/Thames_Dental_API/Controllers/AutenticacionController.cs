using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Thames_Dental_API.Models;

namespace Thames_Dental_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly IConfiguration _conf;

        public AutenticacionController(IConfiguration conf)
        {
            _conf = conf;
        }

        [HttpPost]
        [Route("Registrar")]
        public IActionResult Registrar(UsuarioModel model)
        {
            using (var context = new SqlConnection(_conf.GetSection("ConnectionStrings:DefaultConnection").Value))
            {
                var respuesta = new Respuesta();

                try
                {
                    var result = context.Execute("RegistrarUsuario",
                                                 new { model.Identificacion, model.Nombre, model.Email, model.Contrasena, model.RolID });

                    if (result > 0)
                    {
                        respuesta.Codigo = 0;
                        respuesta.Mensaje = "Usuario registrado correctamente";
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Mensaje = "Error: Los datos no se pudieron registrar.";
                    }
                }
                catch (SqlException ex)
                {
                    respuesta.Codigo = -1;
                    respuesta.Mensaje = "Error: Los datos no se pudieron registrar.";
                }

                return Ok(respuesta);
            }
        }
    }
}
