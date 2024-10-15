using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Thames_Dental_Web.Models
{
    public class ClientModel
    {
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string TelefonoPrincipal { get; set; }
        public string Correo { get; set; }
        public string Pais { get; set; }
        public string Ciudad { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; }
        public string Ocupacion { get; set; }
        public string ContactoEmergencia { get; set; }
        public string TelefonoEmergencia { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Notas { get; set; }

        public string DireccionExacta { get; set; }
    }



}
