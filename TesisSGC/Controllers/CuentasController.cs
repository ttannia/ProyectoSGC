using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace TesisSGC
{
    public class CuentasController : Controller
    {
        private Context db = new Context();

        // GET: Cuentas
        public async Task<ActionResult> Index()
        {
            var cuentas = db.Cuentas
                .Include(c => c.Socio)
                .Include(c => c.CuotasMensuales.Select(cm => cm.UR)); // IMPORTANTE para que funcione TotalPendienteEnPesos

            return View(await cuentas.ToListAsync());
        }

        // GET: Cuentas/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cuenta = await db.Cuentas
                .Include(c => c.Socio)
                .Include(c => c.CuotasMensuales.Select(cm => cm.UR))
                .FirstOrDefaultAsync(c => c.IdCuenta == id);

            if (cuenta == null) return HttpNotFound();

            return View(cuenta);
        }

        // GET: Cuentas/Create
        public ActionResult Create()
        {
            var sociosActivos = db.Socios
                                  .Where(s => s.Estado) // solo activos
                                  .ToList();

            ViewBag.IdCuenta = new SelectList(sociosActivos, "IdSocio", "NombreSocio");
            return View();
        }


        // POST: Cuentas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdCuenta")] Cuenta cuenta)
        {
            ModelState.Remove("IdCuenta");


            if (cuenta.IdCuenta == 0)
            {
                ModelState.AddModelError("IdCuenta", "Debe seleccionar un socio.");
            }

            if (db.Cuentas.Any(c => c.IdCuenta == cuenta.IdCuenta))
            {
                ModelState.AddModelError("", "Este socio ya tiene una cuenta asociada.");
            }

            if (ModelState.IsValid)
            {
                db.Cuentas.Add(cuenta);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdCuenta = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuenta.IdCuenta);
            return View(cuenta);
        }


        // GET: Cuentas/Edit/5

        public async Task<ActionResult> Edit(int id)
        {
            var cuenta = await db.Cuentas
                                 .Include(c => c.Socio)
                                 .FirstOrDefaultAsync(c => c.IdCuenta == id);
            if (cuenta == null)
            {
                return HttpNotFound();
            }
            var sociosActivos = await db.Socios
                                        .Where(s => s.Estado)
                                        .ToListAsync();
            ViewBag.IdSocio = new SelectList(sociosActivos, "IdSocio", "NombreSocio", cuenta.Socio?.IdSocio);
            return View(cuenta);
        }



        // POST: Cuentas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdCuenta")] Cuenta cuenta)
        {
            bool socioConOtraCuenta = db.Cuentas.Any(c => c.IdCuenta != cuenta.IdCuenta && c.Socio.IdSocio == cuenta.Socio.IdSocio);
            if (socioConOtraCuenta)
            {
                ModelState.AddModelError("", "El socio seleccionado ya tiene una cuenta asociada.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(cuenta).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdCuenta = new SelectList(db.Socios, "IdSocio", "NombreSocio", cuenta.IdCuenta);
            return View(cuenta);
        }

        // GET: Cuentas/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cuenta = await db.Cuentas.FindAsync(id);
            if (cuenta == null) return HttpNotFound();

            return View(cuenta);
        }
        // POST: Cuentas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var cuenta = await db.Cuentas.FindAsync(id);
            db.Cuentas.Remove(cuenta);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
