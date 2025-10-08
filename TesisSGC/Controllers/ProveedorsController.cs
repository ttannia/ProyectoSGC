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
    public class ProveedorsController : Controller
    {
        private Context db = new Context();

        // GET: Proveedors
        public async Task<ActionResult> Index()
        {
            return View(await db.Proveedores.ToListAsync());
        }

        // GET: Proveedors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var proveedor = await db.Proveedores
                .FirstOrDefaultAsync(p => p.IdProveedor == id);

            if (proveedor == null)
            {
                return HttpNotFound();
            }

            // Traer pagos realizados al proveedor
            var pagosProveedor = await db.PagosProveedores
                .Where(p => p.IdProveedor == id)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var pagoIds = pagosProveedor.Select(p => p.IdPagoProveedor).ToList();

            var comprobantes = await db.ComprobantesProveedores
            .Where(c => pagoIds.Contains(c.IdPagoProveedor))
            .ToListAsync();

            ViewBag.PagosProveedor = pagosProveedor;
            ViewBag.Comprobantes = comprobantes;

            return View(proveedor);
        }


        // GET: Proveedores/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdProveedor,Nombre,RUT,Direccion,Telefono,Email")] Proveedor proveedor)
        {
            // Validar que el RUT sea único
            bool rutExiste = db.Proveedores.Any(p => p.RUT == proveedor.RUT);
            if (rutExiste)
            {
                ModelState.AddModelError("RUT", "El RUT ya está registrado.");
            }

            if (ModelState.IsValid)
            {
                proveedor.Estado = true; // Se activa por defecto
                db.Proveedores.Add(proveedor);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(proveedor);
        }


        // GET: Proveedores/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = await db.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdProveedor,Nombre,RUT,Direccion,Telefono,Email,Estado")] Proveedor proveedor)
        {
            // Validar que el RUT sea único para otros proveedores
            bool rutExiste = db.Proveedores.Any(p => p.RUT == proveedor.RUT && p.IdProveedor != proveedor.IdProveedor);
            if (rutExiste)
            {
                ModelState.AddModelError("RUT", "El RUT ya está registrado por otro proveedor.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(proveedor).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(proveedor);
        }


        // GET: Proveedors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Proveedor proveedor = await db.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await db.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            // Marca como inactivo
            proveedor.Estado = false;
            db.Entry(proveedor).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Buscar(string query)
        {
            var proveedores = await db.Proveedores.ToListAsync();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower(); // normalizamos a minúsculas

                proveedores = proveedores
                    .Where(p =>
                        p.Nombre != null && p.Nombre.ToLower().Contains(query) ||   // búsqueda insensible a mayúsculas
                        p.RUT.ToString().Contains(query))
                    .ToList();
            }

            return PartialView("_ProveedoresParcial", proveedores);
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
