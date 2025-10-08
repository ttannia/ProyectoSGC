using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class MovimientoCaja
    {
        [Key]
        public int IdMovimiento { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public bool EsEntrada { get; set; }
        public string Medio { get; set; }   // "Efectivo" o "Banco"
        public string Descripcion { get; set; }

        // Relación con caja
        public int IdCaja { get; set; }
        public virtual Caja Caja { get; set; }

        // Relación opcional con pagos
        public int? IdPagoSocio { get; set; }
        public virtual PagoSocio PagoSocio { get; set; }

        public int? IdPagoProveedor { get; set; }
        public virtual PagoProveedor PagoProveedor { get; set; }
    }
}