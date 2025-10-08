using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TesisSGC
{
    public class Cuenta
    {
        [Key, ForeignKey("Socio")]
        public int IdCuenta { get; set; }

        public virtual Socio Socio { get; set; }

        public virtual ICollection<CuotaMensual> CuotasMensuales { get; set; }


        [NotMapped]
        public decimal TotalPendienteEnPesos
        {
            get
            {
                if (CuotasMensuales == null) return 0;

                using (var context = new Context())
                {
                    // Carga los pagos del socio para evitar el problema de contexto
                    var pagosSocio = context.PagosSocios
                                            .Where(p => p.IdSocio == this.Socio.IdSocio)
                                            .ToList();

                    decimal totalPendiente = 0;

                    foreach (var cuota in CuotasMensuales.Where(c => !c.EstaPagada))
                    {
                        // Calcula el monto original de la cuota en pesos
                        var valorUR = context.URs.FirstOrDefault(u => u.Mes == cuota.Mes)?.Monto ?? 0;
                        var montoOriginalEnPesos = cuota.Monto * valorUR;

                        // Suma los pagos que corresponden a esta cuota
                        var pagosParaEstaCuota = pagosSocio
                                                 .Where(p => p.IdCuotaMensual == cuota.IdCuotaMensual)
                                                 .Sum(p => p.Monto);

                        // Calcula el monto pendiente restando los pagos
                        var montoPendientePorCuota = montoOriginalEnPesos - pagosParaEstaCuota;

                        // Suma al total pendiente solo si el monto es mayor que cero
                        if (montoPendientePorCuota > 0)
                        {
                            totalPendiente += montoPendientePorCuota;
                        }
                    }

                    return totalPendiente;
                }
            }
        }

    }
}
