using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class PagoSocio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Clave primaria autoincremental
        public int IdPagoSocio { get; set; }

        [Required]
        public int IdSocio { get; set; }

        public virtual Socio Socio { get; set; }

        [Required]
        public int IdCuotaMensual { get; set; }             
        public virtual CuotaMensual CuotaMensual { get; set; }

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

        [NotMapped]
        public decimal MontoUR
        {
            get
            {
                if (CuotaMensual != null && CuotaMensual.UR != null && CuotaMensual.UR.Monto > 0)
                {
                    return Monto / CuotaMensual.UR.Monto;
                }
                return 0;
            }
        }
    }

}