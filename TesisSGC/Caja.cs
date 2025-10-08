using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class Caja
    {
        [Key]
        public int IdCaja { get; set; }

        public DateTime FechaApertura { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoEfectivo { get; set; }
        public decimal SaldoBanco { get; set; }

        public virtual ICollection<MovimientoCaja> Movimientos { get; set; }
        [NotMapped]
        public decimal SaldoActual => SaldoInicial + SaldoEfectivo + SaldoBanco;
    }
}