using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class Comprobante
    {
        [Key]   // ✅ PK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdComprobante { get; set; }   // Identity incremental

        [Required]   // ✅ no puede ser null
        [ForeignKey("PagoSocio")]  // relaciona con PagoSocio
        public int IdPagoSocio { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; }

        [Required]
        [StringLength(20)]   // ej: "C-00001"
        [Index("IX_NumeroComprobante", IsUnique = true)] // índice único
        public string NumeroComprobante { get; set; }

        public virtual PagoSocio PagoSocio { get; set; }
    }
}