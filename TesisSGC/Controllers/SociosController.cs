using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.SqlServer;

namespace TesisSGC
{
    public class SociosController : Controller
    {
        private Context db = new Context();

        // GET: Socios
        public async Task<ActionResult> Index()
        {
            return View(await db.Socios.ToListAsync());
        }

        // GET: Socios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Incluye la cuenta y las cuotas mensuales para el socio
            var socio = db.Socios
                          .Include(s => s.Cuenta.CuotasMensuales)
                          .FirstOrDefault(s => s.IdSocio == id);

            if (socio == null)
            {
                return HttpNotFound();
            }

            // Traer pagos del socio
            var pagos = db.PagosSocios
                          .Include(p => p.CuotaMensual.UR)
                          .Where(p => p.IdSocio == id)
                          .OrderByDescending(p => p.FechaPago)
                          .ToList();

            // Extraer IDs de los pagos
            var pagoIds = pagos.Select(p => p.IdPagoSocio).ToList();

            // Traer comprobantes de esos pagos
            var comprobantes = db.Comprobantes
                                 .Where(c => pagoIds.Contains(c.IdPagoSocio))
                                 .ToList();

            ViewBag.PagosSocio = pagos;
            ViewBag.Comprobantes = comprobantes;

            return View(socio);
        }
    

    // GET: Socios/Create
    public ActionResult Create()
        {
            return View();
        }

        // POST: Socios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdSocio,NombreSocio,CI,Direccion,TelSocio")] Socio socio)
        {
            // Validar que la CI sea única
            bool ciExiste = db.Socios.Any(s => s.CI == socio.CI);
            if (ciExiste)
            {
                ModelState.AddModelError("CI", "La Cédula de Identidad ya está registrada.");
            }

            if (ModelState.IsValid)
            {
                socio.Estado = true; // Se activa por defecto
                db.Socios.Add(socio);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(socio);
        }


        // GET: Socios/Edit
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Socio socio = await db.Socios.FindAsync(id);
            if (socio == null)
            {
                return HttpNotFound();
            }
            return View(socio);
        }

        // POST: Socios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdSocio,NombreSocio,CI,Direccion,TelSocio,Estado")] Socio socio)
        {
            // Validar que la CI sea única para otros socios (excluyendo el actual)
            bool ciExiste = db.Socios.Any(s => s.CI == socio.CI && s.IdSocio != socio.IdSocio);
            if (ciExiste)
            {
                ModelState.AddModelError("CI", "La Cédula de Identidad ya está registrada por otro socio.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(socio).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(socio);
        }

        // GET: Socios/Delete
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Socio socio = await db.Socios.FindAsync(id);
            if (socio == null)
            {
                return HttpNotFound();
            }
            return View(socio);
        }

        // POST: Socios/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var socio = await db.Socios.FindAsync(id);
            if (socio == null)
            {
                return HttpNotFound();
            }
            // Verifica si tiene cuotas impagas
            bool tieneCuotasImpagas = db.CuotasMensuales
                .Any(c => c.IdSocio == id && !c.EstaPagada);

            if (tieneCuotasImpagas)
            {
                TempData["ErrorMessage"] = "No se puede dar de baja al socio porque tiene cuotas impagas.";
                return RedirectToAction("Index", new { id });
            }

            // Marca como inactivo
            socio.Estado = false;
            db.Entry(socio).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Buscar(string query)
        {
            var socios = await db.Socios
                .Where(s =>
                    s.NombreSocio.Contains(query) ||
                    SqlFunctions.StringConvert((double)s.CI).Trim().Contains(query))
                .ToListAsync();

            return PartialView("_SociosParcial", socios);
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
