using System.ComponentModel.DataAnnotations;

namespace InventarioAPI.Models
{
    public class Inventario
    {

        public int IdInventario { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [StringLength(50)]
        public string Proveedor { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; }

        [Required]
        public bool Activo { get; set; } // Campo para eliminado lógico

    }
}
