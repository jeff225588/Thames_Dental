namespace Thames_Dental_API.Models
{
    public class UsuarioModel
    {
        public long UsuarioId { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public bool UsaClaveTemporal { get; set; }
        public DateTime Vigencia { get; set; }
        public short RolID { get; set; } = 3; // Rol predeterminado de "Cliente"
        public string NombreRol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
