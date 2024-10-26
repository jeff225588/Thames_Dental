namespace Thames_Dental_Web.Models
{
    public class CitaModel
    {
        public int Id { get; set; }
        public int ClienteID { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Especialista { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Procedimiento { get; set; } = string.Empty;
        public int Duracion { get; set; } // Duración en minutos
    }
}
