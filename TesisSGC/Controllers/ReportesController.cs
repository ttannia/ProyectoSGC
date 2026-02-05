using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TesisSGC.ViewModels;
using System.Web.UI;

namespace TesisSGC.Controllers
{
    public class ReportesController : Controller
    {
        private readonly Context db;

        public ReportesController()
        {
            db = new Context();
        }

        public ActionResult Index()
        {
            var vm = new FiltroReporteVM
            {
                Socios = db.Socios
                    .Where(s => s.Estado) // solo activos
                    .Select(s => new SelectListItem { Value = s.IdSocio.ToString(), Text = s.NombreSocio }),
                Proveedores = db.Proveedores
                .Select(p => new SelectListItem { Value = p.IdProveedor.ToString(), Text = p.Nombre })
            };
            return View(vm);
        }

        public ActionResult ExportarPdf(FiltroReporteVM filtros)
        {
            try
            {
                // Reutilizar la lógica de generación de datos
                object datos = null;
                switch (filtros.TipoReporte)
                {
                    case "Pagos":
                        datos = db.PagosSocios
                            .Include(p => p.Socio)
                            .Include(p => p.CuotaMensual)
                            .Where(p =>
                                (!filtros.Mes.HasValue ||
                                    (p.FechaPago.Year == filtros.Mes.Value.Year &&
                                     p.FechaPago.Month == filtros.Mes.Value.Month)) &&
                                (!filtros.IdSocio.HasValue || p.IdSocio == filtros.IdSocio))
                            .ToList();
                        if (filtros.IdSocio.HasValue)
                        {
                            var socio = db.Socios.FirstOrDefault(s => s.IdSocio == filtros.IdSocio.Value);
                            if (socio != null)
                            {
                                ViewBag.NombreSocio = socio.NombreSocio;
                                ViewBag.CI = socio.CI;
                            }
                        }
                        break;
                    case "Socios":
                        datos = db.Socios
                                  .OrderByDescending(s => s.Estado)
                                  .ThenBy(s => s.NombreSocio)
                                  .ToList();
                        break;
                    case "Cuotas":
                        datos = db.CuotasMensuales
                                  .Include(c => c.Socio)
                                  .Include(c => c.UR)
                                  .Where(c =>
                                    (!filtros.Mes.HasValue ||
                                        (c.Mes.Year == filtros.Mes.Value.Year &&
                                         c.Mes.Month == filtros.Mes.Value.Month)) &&
                                    (!filtros.IdSocio.HasValue || c.IdSocio == filtros.IdSocio))
                                  .ToList();

                        if (filtros.IdSocio.HasValue)
                        {
                            var socio = db.Socios.FirstOrDefault(s => s.IdSocio == filtros.IdSocio.Value);
                            if (socio != null)
                            {
                                ViewBag.NombreSocio = socio.NombreSocio;
                                ViewBag.CI = socio.CI;
                            }
                        }
                        break;

                    case "Caja":
                        datos = db.Cajas.Include(c => c.Movimientos).ToList();
                        break;

                    case "Proveedores":
                        datos = db.Proveedores
                                  .OrderByDescending(p => p.Estado)
                                  .ThenBy(p => p.Nombre)
                                  .ToList();
                        break;

                    case "PagosProv":
                        datos = db.PagosProveedores
                                    .Include(p => p.Proveedor)
                                    .Where(p =>
                                        (!filtros.Mes.HasValue ||
                                            (p.FechaPago.Year == filtros.Mes.Value.Year &&
                                             p.FechaPago.Month == filtros.Mes.Value.Month)) &&
                                        (!filtros.IdProveedor.HasValue || p.IdProveedor == filtros.IdProveedor))
                                    .ToList();

                        if (filtros.IdProveedor.HasValue)
                        {
                            var proveedor = db.Proveedores.FirstOrDefault(p => p.IdProveedor == filtros.IdProveedor.Value);
                            if (proveedor != null)
                            {
                                ViewBag.NombreProveedor = proveedor.Nombre;
                                ViewBag.RUTProveedor = proveedor.RUT;
                            }
                        }
                        break;

                }

                filtros.DatosReporte = datos;
                filtros.Socios = db.Socios
                    .Where(s => s.Estado)
                    .Select(s => new SelectListItem { Value = s.IdSocio.ToString(), Text = s.NombreSocio });
                filtros.Proveedores = db.Proveedores
                .Where(p => p.Estado)
                .Select(p => new SelectListItem { Value = p.IdProveedor.ToString(), Text = p.Nombre });


                ViewBag.Mes = filtros.Mes?.Month;
                ViewBag.Anio = filtros.Mes?.Year;
                ViewBag.IdSocio = filtros.IdSocio;
                ViewBag.IdProveedor = filtros.IdProveedor;



                return new Rotativa.ViewAsPdf("GenerarReporte", filtros)
                {
                    FileName = $"{filtros.TipoReporte}.pdf",
                    PageSize = Rotativa.Options.Size.A4
                };

            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al generar el reporte. Detalle: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        }

}