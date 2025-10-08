using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TesisSGC;
using System.Data.Entity.SqlServer;
using TesisSGC.ViewModels;

namespace TesisSGC
{
    public class CuotaMensualsController : Controller
    {
        private Context db = new Context();

        // GET: CuotaMensuals
        public async Task<ActionResult> Index()
        {
            var cuotasMensuales = db.CuotasMensuales.Include(c => c.Cuenta).Include(c => c.Socio).Include(c => c.UR);
            return View(await cuotasMensuales.ToListAsync());
        }

        // GET: CuotaMensuals/Details
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CuotaMensual cuotaMensual = await db.CuotasMensuales.FindAsync(id);
            if (cuotaMensual == null)
            {
                return HttpNotFound();
            }
            return View(cuotaMensual);
        }

        // GET: CuotaMensuals/Create
        public ActionResult Create()
        {
            ViewBag.IdCuenta = new SelectList(db.Cuentas, "IdCuenta", "IdCuenta");
            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio");
            ViewBag.IdUR = new SelectList(db.URs, "IdUR", "Mes");
            return View();
        }

        // POST: CuotaMensuals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdCuotaMensual,IdSocio,Mes,Monto,EstaPagada,Tipo")] CuotaMensual cuotaMensual)
        {
            if (ModelState.IsValid)
            {
                // Buscar cuenta asociada
                var cuenta = await db.Cuentas.FirstOrDefaultAsync(c => c.Socio.IdSocio == cuotaMensual.IdSocio);
                if (cuenta == null)
                {
                    ModelState.AddModelError("", "El socio no tiene una cuenta asociada.");
                    ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuotaMensual.IdSocio);
                    ViewBag.TipoCuota = new SelectList(new[] { "ingreso", "vivienda", "gastos comunes" }, cuotaMensual.Tipo);
                    return View(cuotaMensual);
                }

                // Verificación de existencia de UR para el mes
                var ur = await db.URs.FirstOrDefaultAsync(u => u.Mes == cuotaMensual.Mes);
                if (ur == null)
                {
                    ModelState.AddModelError("", "No existe un valor UR para el mes seleccionado.");
                    ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuotaMensual.IdSocio);
                    ViewBag.TipoCuota = new SelectList(new[] { "ingreso", "vivienda", "gastos comunes" }, cuotaMensual.Tipo);
                    return View(cuotaMensual);
                }

                // Validaciones según tipo
                var otrasCuotas = db.CuotasMensuales.Where(c => c.IdSocio == cuotaMensual.IdSocio);

                if (cuotaMensual.Tipo == "ingreso")
                {
                    if (await otrasCuotas.AnyAsync(c => c.Tipo == "ingreso"))
                    {
                        ModelState.AddModelError("Tipo", "El socio ya tiene una cuota de ingreso registrada.");
                    }
                }
                else if (cuotaMensual.Tipo == "vivienda")
                {
                    if (await otrasCuotas.AnyAsync(c => c.Tipo == "vivienda" &&
                         DbFunctions.TruncateTime(c.Mes) == DbFunctions.TruncateTime(cuotaMensual.Mes)))
                    {
                        ModelState.AddModelError("Tipo", "Ya existe una cuota de vivienda para este socio en el mes indicado.");
                    }
                }
                else if (cuotaMensual.Tipo == "gastos comunes")
                {
                    if (await db.CuotasMensuales.AnyAsync(c => c.Tipo == "gastos comunes" &&
                         DbFunctions.TruncateTime(c.Mes) == DbFunctions.TruncateTime(cuotaMensual.Mes)))
                    {
                        ModelState.AddModelError("Tipo", "Ya existe una cuota de gastos comunes para ese mes.");
                    }
                }

                if (string.IsNullOrWhiteSpace(cuotaMensual.Tipo))
                {
                    ModelState.AddModelError("Tipo", "Debe seleccionar un tipo de cuota.");
                }   

                if (!ModelState.IsValid)
                {
                    ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuotaMensual.IdSocio);
                    ViewBag.TipoCuota = new SelectList(new[] { "ingreso", "vivienda", "gastos comunes" }, cuotaMensual.Tipo);
                    return View(cuotaMensual);
                }

                cuotaMensual.IdCuenta = cuenta.IdCuenta;
                cuotaMensual.IdUR = ur.IdUR;

                cuotaMensual.SaldoPendiente = cuotaMensual.Monto;
                cuotaMensual.EstaPagada = false; 


                db.CuotasMensuales.Add(cuotaMensual);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuotaMensual.IdSocio);
            ViewBag.TipoCuota = new SelectList(new[] { "ingreso", "vivienda", "gastos comunes" }, cuotaMensual.Tipo);
            return View(cuotaMensual);
        }

        // GET: CuotaMensuals/Edit
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cuota = await db.CuotasMensuales.FindAsync(id);
            if (cuota == null)
                return HttpNotFound();

            var viewModel = new CuotaMensualEditViewModel
            {
                IdCuotaMensual = cuota.IdCuotaMensual,
                IdSocio = cuota.IdSocio,
                Mes = cuota.Mes,
                Tipo = cuota.Tipo,
                Monto = cuota.Monto.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                EstaPagada = cuota.EstaPagada,

                Socios = new SelectList(
                    db.Socios.Where(s => s.Estado).ToList(),
                    "IdSocio",
                    "NombreSocio",
                    cuota.IdSocio // <-- socio seleccionado
                )
            };

            return View(viewModel);
        }

        // POST: CuotaMensuals/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CuotaMensualEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar el dropdown con selección correcta si hay errores
                viewModel.Socios = db.Socios
                                     .Where(s => s.Estado)
                                     .Select(s => new SelectListItem
                                     {
                                         Value = s.IdSocio.ToString(),
                                         Text = s.NombreSocio,
                                         Selected = (s.IdSocio == viewModel.IdSocio)
                                     })
                                     .ToList();

                return View(viewModel);
            }

            var cuota = await db.CuotasMensuales.FindAsync(viewModel.IdCuotaMensual);
            if (cuota == null)
                return HttpNotFound();

            // Mapear valores del ViewModel a la entidad
            cuota.IdSocio = viewModel.IdSocio;
            cuota.Mes = viewModel.Mes;
            cuota.Tipo = viewModel.Tipo;

            decimal nuevoMonto = decimal.Parse(viewModel.Monto.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);

            cuota.Monto = nuevoMonto;

            // ⚡ Recalcular SaldoPendiente considerando pagos realizados
            var totalPagado = db.PagosSocios
                                .Where(p => p.IdCuotaMensual == cuota.IdCuotaMensual)
                                .Select(p => (decimal?)p.Monto)
                                .Sum() ?? 0m;
            // Traer el valor de la UR más reciente
            var valorUR = db.URs
                            .OrderByDescending(u => u.Mes)
                            .Select(u => (decimal?)u.Monto)
                            .FirstOrDefault() ?? 1400m;

            // Convertir el monto de la cuota a pesos
            decimal nuevoMontoEnPesos = nuevoMonto * valorUR;

            // Calcular totalPagado en pesos (ya lo tenés)
            var totalPagadoEnPesos = totalPagado;

            // Calcular saldo pendiente en pesos
            cuota.SaldoPendiente = Math.Max(nuevoMontoEnPesos - totalPagadoEnPesos, 0);


            /*decimal nuevoMontoEnPesos = nuevoMonto * valorUR;
            cuota.SaldoPendiente = Math.Max(nuevoMontoEnPesos - totalPagado, 0);*/

            // ⚡ Determinar estado real de la cuota según pagos
            bool estaRealmentePagada = cuota.SaldoPendiente == 0;

            // ⚡ Validación de coherencia entre saldo y estado (mensajes intactos)
            if (viewModel.EstaPagada && !estaRealmentePagada)
            {
                ModelState.AddModelError("EstaPagada", "No se puede marcar como pagada porque aún hay saldo pendiente.");
            }
            else if (!viewModel.EstaPagada && estaRealmentePagada)
            {
                ModelState.AddModelError("EstaPagada", "No se puede marcar como no pagada porque la cuota ya está saldada.");
            }


            if (!ModelState.IsValid)
            {
                // Repoblar el dropdown
                viewModel.Socios = db.Socios
                                     .Where(s => s.Estado)
                                     .Select(s => new SelectListItem
                                     {
                                         Value = s.IdSocio.ToString(),
                                         Text = s.NombreSocio,
                                         Selected = (s.IdSocio == viewModel.IdSocio)
                                     })
                                     .ToList();

                return View(viewModel);
            }

            cuota.EstaPagada = estaRealmentePagada;


            db.Entry(cuota).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // GET: CuotaMensuals/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CuotaMensual cuotaMensual = await db.CuotasMensuales.FindAsync(id);
            if (cuotaMensual == null)
            {
                return HttpNotFound();
            }
            return View(cuotaMensual);
        }

        // POST: CuotaMensuals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            CuotaMensual cuotaMensual = await db.CuotasMensuales.FindAsync(id);
            db.CuotasMensuales.Remove(cuotaMensual);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> BuscarCuotas(string query)
        {
            var cuotas = db.CuotasMensuales.Include(c => c.Socio);

            if (!string.IsNullOrEmpty(query))
            {
                cuotas = cuotas.Where(c =>
                    c.Socio.NombreSocio.Contains(query) ||
                    SqlFunctions.StringConvert((double)c.Socio.CI).Trim().Contains(query) ||
                    c.Tipo.Contains(query));
                    
            }

            var resultados = await cuotas
                .OrderByDescending(c => c.Mes) // del más nuevo al más viejo
                .ToListAsync();

            return PartialView("_CuotasParcial", resultados);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
