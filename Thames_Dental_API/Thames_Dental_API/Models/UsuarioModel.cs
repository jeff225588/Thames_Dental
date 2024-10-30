namespace Thames_Dental_API.Models
{
    public class UsuarioModel
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int RolID { get; set; } = 3; // Rol predeterminado de "Cliente"
    }
}
