using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisSGC
{
    public class HistorialPagoProveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int IdPagoProveedor { get; set; } // vínculo al pago original

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        [Required]
        public decimal Monto { get; set; }

        [Required]
        public string FormaPago { get; set; }

        [Required]
        public string TipoPago { get; set; }

        [MaxLength(100)]
        public string NumeroTransferencia { get; set; }

        [MaxLength(100)]
        public string NumeroCheque { get; set; }

        [MaxLength(100)]
        public string NumeroTransaccion { get; set; }

        public string Observaciones { get; set; }

        // --- Datos del historial ---
        [Required]
        [MaxLength(20)]
        public string Accion { get; set; } // Creado, Editado, Eliminado

        [Required]
        public DateTime FechaAccion { get; set; }
    }
}
