﻿using System.ComponentModel.DataAnnotations;

namespace Thames_Dental_API.Models
{
    public class Inventario
    {

        public int IdInventario { get; set; }


        public int IdAuditoria { get; set; }

        public string Accion { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaCambio { get; set; }




        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int Cantidad { get; set; }

        [StringLength(50)]
        public string Proveedor { get; set; } = string.Empty;

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
