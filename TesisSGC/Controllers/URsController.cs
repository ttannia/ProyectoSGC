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

namespace TesisSGC
{
    public class URsController : Controller
    {
        private Context db = new Context();

        // GET: URs
        public async Task<ActionResult> Index()
        {
            return View(await db.URs.ToListAsync());
        }

        // GET: URs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UR uR = await db.URs.FindAsync(id);
            if (uR == null)
            {
                return HttpNotFound();
            }
            return View(uR);
        }

        // GET: URs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: URs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUR,Mes,Monto")] UR uR)
        {
            // Validar que no exista otro UR con el mismo Mes
            if (db.URs.Any(x => x.Mes == uR.Mes))
            {
                ModelState.AddModelError("Mes", "Ya existe una UR para ese mes.");
            }

            if (ModelState.IsValid)
            {
                db.URs.Add(uR);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(uR);
        }

        // GET: URs/Edit
        public async Task<ActionResult> Edit(int? id)
        {
            UR uR_entidad = await db.URs.FindAsync(id);
            if (uR_entidad == null) return HttpNotFound();

            var viewModel = new UREditViewModel
            {
                IdUR = uR_entidad.IdUR,
                Mes = uR_entidad.Mes,
                // Convertimos el decimal de la BD a string para la vista
                Monto = uR_entidad.Monto.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };

            return View(viewModel);
        }

        // POST: URs/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UREditViewModel viewModel)
        {
            bool mesDuplicado = db.URs.Any(x => x.Mes == viewModel.Mes && x.IdUR != viewModel.IdUR);
            if (mesDuplicado)
            {
                ModelState.AddModelError("Mes", "Ya existe una UR registrada para ese mes.");
            }

            if (ModelState.IsValid)
            {
                UR uR_a_modificar = await db.URs.FindAsync(viewModel.IdUR);

                uR_a_modificar.Mes = viewModel.Mes;

                // --- CONVERSIÓN DE VUELTA ---
                // Convertimos el string validado de vuelta a decimal para guardarlo
                // Usamos Replace para asegurar que el punto sea el separador decimal
                uR_a_modificar.Monto = decimal.Parse(viewModel.Monto.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);

                db.Entry(uR_a_modificar).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }


        // GET: URs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UR uR = await db.URs.FindAsync(id);
            if (uR == null)
            {
                return HttpNotFound();
            }
            return View(uR);
        }

        // POST: URs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            UR uR = await db.URs.FindAsync(id);
            db.URs.Remove(uR);
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
