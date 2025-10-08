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

namespace TesisSGC.Controllers
{
    public class CajaController : Controller
    {
        private Context db = new Context();

        // GET: Caja
        public ActionResult Index()
        {
            var caja = db.Cajas
                .Include(c => c.Movimientos)
                .FirstOrDefault(c => c.IdCaja == 1); // suponemos 1 caja única

            if (caja == null)
            {
                return HttpNotFound("No existe caja");
            }

            return View(caja);
        }

        // POST: Caja/EditarSaldoInicial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarSaldoInicial(int IdCaja, decimal nuevoSaldoInicial)
        {
            try
            {
                var caja = db.Cajas.Find(IdCaja);
                if (caja == null)
                    return HttpNotFound("La caja especificada no existe.");

                caja.SaldoInicial = nuevoSaldoInicial;

                // 🚨 recalcular saldos
                caja.SaldoEfectivo = caja.Movimientos
                    .Where(m => m.Medio == "Efectivo")
                    .Sum(m => m.EsEntrada ? m.Monto : -m.Monto);

                caja.SaldoBanco = caja.Movimientos
                    .Where(m => m.Medio == "Banco")
                    .Sum(m => m.EsEntrada ? m.Monto : -m.Monto);

                db.SaveChanges();

                TempData["Mensaje"] = "Saldo inicial actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el saldo inicial: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


    }

}