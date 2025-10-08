using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisSGC
{
    public class PagoProveedor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPagoProveedor { get; set; }

        [Required]
        public int IdProveedor { get; set; }
        public virtual Proveedor Proveedor { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        [Required]
        public decimal Monto { get; set; }

        [Required]
        public string FormaPago { get; set; } // Efectivo, Transferencia, Cheque, etc.

        [Required]
        public string TipoPago { get; set; } 

        [MaxLength(100)]
        public string NumeroTransferencia { get; set; }

        [MaxLength(100)]
        public string NumeroCheque { get; set; }

        [MaxLength(100)]
        public string NumeroTransaccion { get; set; }

        public string Observaciones { get; set; }

    }
}
