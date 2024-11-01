namespace Thames_Dental_API.Models
{
    public class Cliente
    {
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string TelefonoPrincipal { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string DireccionExacta { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Ocupacion { get; set; } = string.Empty;
        public string ContactoEmergencia { get; set; } = string.Empty;
        public string TelefonoEmergencia { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string Notas { get; set; } = string.Empty;
    }


    public class Nota
    {

        public DateTime Fecha { get; set; }
        public string Detalle { get; set; } = string.Empty; // Detalle de la nota
        public string Cedula { get; set; } = string.Empty; // Campo para la cédula
    }

    public class Autorizado
    {
        public string Cedula { get; set; } = string.Empty; // Cédula del cliente
        public string Nombre { get; set; } = string.Empty; // Nombre del autorizado
        public string TelefonoEmergencia { get; set; } = string.Empty; // Teléfono de emergencia
    }
}

