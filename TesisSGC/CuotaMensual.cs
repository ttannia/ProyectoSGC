using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class CuotaMensual
    {
        [Key]
        public int IdCuotaMensual { get; set; }

        public int IdUR { get; set; }

        public int IdSocio { get; set; }

        public DateTime Mes { get; set; }

        public decimal Monto { get; set; }

        public int IdCuenta { get; set; }

        [ForeignKey("IdUR")]
        public virtual UR UR { get; set; }

        [ForeignKey("IdSocio")]
        public virtual Socio Socio { get; set; }

        [ForeignKey("IdCuenta")]
        public virtual Cuenta Cuenta { get; set; }
    }
}