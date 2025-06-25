using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesisSGC
{
    public class Cuenta
    {
        [Key, ForeignKey("Socio")]
        public int IdCuenta { get; set; }

        public int CuotaPendiente { get; set; }

        public virtual Socio Socio { get; set; }

        public virtual ICollection<CuotaMensual> CuotasMensuales { get; set; }
    }
}
