﻿using System.ComponentModel.DataAnnotations;

namespace Thames_Dental_Web.Models
{
    public class UsuarioModel
    {
        public long UsuarioId { get; set; }
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;
        public string ConfirmarContrasena { get; set; } = string.Empty;
        public short RolID { get; set; } = 3; // Rol predeterminado de "Cliente"
        public string NombreRol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
