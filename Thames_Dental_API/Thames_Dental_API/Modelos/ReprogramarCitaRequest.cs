using System.ComponentModel.DataAnnotations;

namespace Thames_Dental_API.Modelos
{
    public class ReprogramarCitaRequest
    {
        [Required(ErrorMessage = "El ID de la cita es obligatorio.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date, ErrorMessage = "La fecha no tiene un formato válido.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria.")]
        [DataType(DataType.Time, ErrorMessage = "La hora no tiene un formato válido.")]
        public TimeSpan Hora { get; set; }

        [Range(1, 240, ErrorMessage = "La duración debe estar entre 1 y 240 minutos.")]
        public int Duracion { get; set; }
    }
}
