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

        public string Tipo { get; set; }

        public decimal Monto { get; set; }

        public bool EstaPagada { get; set; } = false;

        public int IdCuenta { get; set; }

        public decimal SaldoPendiente { get; set; }

        [ForeignKey("IdUR")]
        public virtual UR UR { get; set; }

        [ForeignKey("IdSocio")]
        public virtual Socio Socio { get; set; }

        [ForeignKey("IdCuenta")]
        public virtual Cuenta Cuenta { get; set; }

        public virtual ICollection<PagoSocio> PagoSocios { get; set; }

    }
}