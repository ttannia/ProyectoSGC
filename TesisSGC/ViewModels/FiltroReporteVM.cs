using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TesisSGC.ViewModels
{
    public class FiltroReporteVM
    {
        public string TipoReporte { get; set; }
        public DateTime? Mes { get; set; }
        public int? IdSocio { get; set; }
        public int? IdProveedor { get; set; }
        public IEnumerable<SelectListItem> Socios { get; set; }

        public object DatosReporte { get; set; }

        public string NombreSocio { get; set; }
        public string CI { get; set; }

        public IEnumerable<SelectListItem> Proveedores { get; set; }

    }

}