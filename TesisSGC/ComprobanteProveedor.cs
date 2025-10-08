using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class ComprobanteProveedor
    {
        [Key]
        public int IdComprobanteProveedor { get; set; }

        [Required]
        [ForeignKey("PagoProveedor")]
        public int IdPagoProveedor { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; }

        [Required]
        [StringLength(20)]
        public string NumeroComprobante { get; set; }

        public virtual PagoProveedor PagoProveedor { get; set; }
    }
}