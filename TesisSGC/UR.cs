using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TesisSGC
{
    public class UR
    {
        [Key]
        public int IdUR { get; set; }

        public DateTime Mes { get; set; }

        public decimal Monto { get; set; }

        public virtual ICollection<CuotaMensual> CuotasMensuales { get; set; }
    }
}