using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using TesisSGC.ViewModels;

namespace TesisSGC
{
    public class PagoProveedorsController : Controller
    {
        private Context db = new Context();

        // GET: PagoProveedors
        public async Task<ActionResult> Index()
        {
            var pagosProveedores = db.PagosProveedores.Include(p => p.Proveedor);
            return View(await pagosProveedores.ToListAsync());
        }

        // GET: PagoProveedors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            try
            {
                var pagoProveedor = await db.PagosProveedores
                    .Include(p => p.Proveedor)
                    .FirstOrDefaultAsync(p => p.IdPagoProveedor == id);

                if (pagoProveedor == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el pago del proveedor.";
                    return RedirectToAction("Index");
                }

                var comprobante = await db.ComprobantesProveedores
                    .FirstOrDefaultAsync(c => c.IdPagoProveedor == pagoProveedor.IdPagoProveedor);

                ViewBag.ComprobanteId = comprobante?.IdComprobanteProveedor;

                return View(pagoProveedor);
            }
            catch (Exception ex)
            {
                // ⚡ Log para debugging
                System.Diagnostics.Debug.WriteLine($"Error al obtener detalles del pago del proveedor: {ex.Message}");

                TempData["ErrorMessage"] = "Ocurrió un error al obtener los detalles del pago. Inténtelo nuevamente.";
                return RedirectToAction("Index");
            }
        }



        // GET: PagoProveedors/Create
        public ActionResult Create(int? idProveedor)
        {
            if (idProveedor == null)
            {
                return RedirectToAction("SeleccionarProveedorParaPago");
            }

            var proveedor = db.Proveedores.Find(idProveedor);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            var pago = new PagoProveedor
            {
                IdProveedor = proveedor.IdProveedor,
                FechaPago = DateTime.Now
            };

            ViewBag.NombreProveedor = proveedor.Nombre;
            ViewBag.RUTProveedor = proveedor.RUT; 

            return View(pago);
        }


        // POST: PagoProveedors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdProveedor,FechaPago,Monto,FormaPago,TipoPago,NumeroTransferencia,NumeroCheque,NumeroTransaccion,Observaciones")] PagoProveedor pagoProveedor)
        {

            // Validar monto
            if (pagoProveedor.Monto <= 0)
                ModelState.AddModelError("Monto", "El monto debe ser mayor a 0.");

            // Validar que el número no sea nulo según la FormaPago
            if (pagoProveedor.FormaPago == "Transferencia")
            {
                if (string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransferencia))
                    ModelState.AddModelError("NumeroTransferencia", "Debe ingresar el número de transferencia.");
            }

            if (pagoProveedor.FormaPago == "Cheque")
            {
                if (string.IsNullOrWhiteSpace(pagoProveedor.NumeroCheque))
                    ModelState.AddModelError("NumeroCheque", "Debe ingresar el número de cheque.");
            }

            if (pagoProveedor.FormaPago == "Débito")
            {
                if (string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransaccion))
                    ModelState.AddModelError("NumeroTransaccion", "Debe ingresar el número de transacción.");
            }

            // Transferencia
            if (!string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransferencia))
            {
                if (!long.TryParse(pagoProveedor.NumeroTransferencia, out _))
                {
                    ModelState.AddModelError("NumeroTransferencia", "Debe ser un número válido.");
                }
                else
                {
                    bool transferenciaDuplicada = db.PagosProveedores
                        .Any(p => p.NumeroTransferencia == pagoProveedor.NumeroTransferencia);
                    if (transferenciaDuplicada)
                        ModelState.AddModelError("NumeroTransferencia", "Este número de transferencia ya está registrado.");
                }
            }

            // Cheque
            if (!string.IsNullOrWhiteSpace(pagoProveedor.NumeroCheque))
            {
                if (!long.TryParse(pagoProveedor.NumeroCheque, out _))
                {
                    ModelState.AddModelError("NumeroCheque", "Debe ser un número válido.");
                }
                else
                {
                    bool chequeDuplicado = db.PagosProveedores
                        .Any(p => p.NumeroCheque == pagoProveedor.NumeroCheque);
                    if (chequeDuplicado)
                        ModelState.AddModelError("NumeroCheque", "Este número de cheque ya está registrado.");
                }
            }

            // Transacción
            if (!string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransaccion))
            {
                if (!long.TryParse(pagoProveedor.NumeroTransaccion, out _))
                {
                    ModelState.AddModelError("NumeroTransaccion", "Debe ser un número válido.");
                }
                else
                {
                    bool transaccionDuplicada = db.PagosProveedores
                        .Any(p => p.NumeroTransaccion == pagoProveedor.NumeroTransaccion);
                    if (transaccionDuplicada)
                        ModelState.AddModelError("NumeroTransaccion", "Este número de transacción ya está registrado.");
                }
            }
            

            // Solo guardar si ModelState es válido
            if (ModelState.IsValid)
            {
                // Limpiar campos vacíos (convertir a null)
                pagoProveedor.NumeroTransferencia = string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransferencia) ? null : pagoProveedor.NumeroTransferencia;
                pagoProveedor.NumeroCheque = string.IsNullOrWhiteSpace(pagoProveedor.NumeroCheque) ? null : pagoProveedor.NumeroCheque;
                pagoProveedor.NumeroTransaccion = string.IsNullOrWhiteSpace(pagoProveedor.NumeroTransaccion) ? null : pagoProveedor.NumeroTransaccion;

                db.PagosProveedores.Add(pagoProveedor);
                await db.SaveChangesAsync();

                try
                {
                    var yaExiste = db.ComprobantesProveedores.FirstOrDefault(c => c.IdPagoProveedor == pagoProveedor.IdPagoProveedor);
                    if (yaExiste == null)
                    {
                        int siguienteId = db.ComprobantesProveedores.Any() ? db.ComprobantesProveedores.Max(c => c.IdComprobanteProveedor) + 1 : 1;
                        string numeroComprobante = $"P-{siguienteId:D5}";

                        var comprobante = new ComprobanteProveedor
                        {
                            IdPagoProveedor = pagoProveedor.IdPagoProveedor,
                            FechaEmision = DateTime.Now,
                            NumeroComprobante = numeroComprobante
                        };

                        db.ComprobantesProveedores.Add(comprobante);
                        await db.SaveChangesAsync();

                        return RedirectToAction("DescargarComprobanteYRedirigir", new { id = comprobante.IdComprobanteProveedor });
                    }
                    else
                    {
                        return RedirectToAction("DescargarComprobanteYRedirigir", new { id = yaExiste.IdComprobanteProveedor });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al generar comprobante: {ex.Message}");
                    TempData["ErrorMessage"] = "El pago fue registrado, pero no se pudo generar el comprobante.";
                    return RedirectToAction("Details", new { id = pagoProveedor.IdPagoProveedor });
                }
            }


            // Si hay errores, recargar lista de proveedores manteniendo la selección
            ViewBag.IdProveedor = new SelectList(db.Proveedores, "IdProveedor", "Nombre", pagoProveedor.IdProveedor);
            return View(pagoProveedor);
        }

        // ✅ NUEVA ACCIÓN
        public ActionResult Comprobante(int id)
        {
            try
            {
                // Buscar comprobante con sus relaciones
                var comprobante = db.ComprobantesProveedores
                    .Include(c => c.PagoProveedor)
                    .Include(c => c.PagoProveedor.Proveedor)
                    .SingleOrDefault(c => c.IdComprobanteProveedor == id);

                if (comprobante == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el comprobante solicitado.";
                    return RedirectToAction("Details", new { id = id });
                }

                string fileName = $"Comprobante_Proveedor_{comprobante.NumeroComprobante}.pdf";

                return new Rotativa.ViewAsPdf("ComprobanteProveedor", comprobante)
                {
                    FileName = fileName,
                    PageSize = Rotativa.Options.Size.A4,
                    PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10),
                    CustomSwitches = "--print-media-type --disable-smart-shrinking"
                };
            }
            catch (Exception ex)
            {
                // Log para debugging
                System.Diagnostics.Debug.WriteLine($"Error al generar comprobante: {ex.Message}");

                TempData["ErrorMessage"] = "Ocurrió un error al generar el comprobante. Inténtelo nuevamente.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // ✅ Versión para proveedor
        public ActionResult DescargarComprobanteYRedirigir(int id)
        {
            ViewBag.ComprobanteId = id;
            return View("DescargarComprobanteYRedirigir");
        }


        // GET: PagoProveedors/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var pagoProveedor = await db.PagosProveedores
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.IdPagoProveedor == id);

            if (pagoProveedor == null)
                return HttpNotFound();

            var viewModel = new PagoProveedorEditViewModel
            {
                IdPagoProveedor = pagoProveedor.IdPagoProveedor,
                IdProveedor = pagoProveedor.IdProveedor,
                FechaPago = pagoProveedor.FechaPago,
                Monto = pagoProveedor.Monto.ToString("0.##", CultureInfo.InvariantCulture),
                FormaPago = pagoProveedor.FormaPago,
                TipoPago = pagoProveedor.TipoPago,
                NumeroTransferencia = pagoProveedor.NumeroTransferencia,
                NumeroCheque = pagoProveedor.NumeroCheque,
                NumeroTransaccion = pagoProveedor.NumeroTransaccion,
                Observaciones = pagoProveedor.Observaciones,
                NombreProveedor = pagoProveedor.Proveedor?.Nombre,
                RUTProveedor = pagoProveedor.Proveedor.RUT.ToString()

            };

            ViewBag.IdProveedor = new SelectList(db.Proveedores, "IdProveedor", "Nombre", pagoProveedor.IdProveedor);

            return View(viewModel);
        }


        // POST: PagoProveedors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PagoProveedorEditViewModel viewModel)
        {
            // Validar monto
            decimal montoDecimal = 0;
            if (!viewModel.TryGetMontoDecimal(out montoDecimal) || montoDecimal <= 0)
                ModelState.AddModelError("Monto", "El Monto debe ser un número válido mayor a 0 (ej: 123 o 123,45).");

            // Traer pago original
            var pagoOriginal = await db.PagosProveedores
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.IdPagoProveedor == viewModel.IdPagoProveedor);

            if (pagoOriginal == null)
                return HttpNotFound();

            // Validar campos obligatorios según FormaPago
            if (viewModel.FormaPago == "Transferencia" && string.IsNullOrWhiteSpace(viewModel.NumeroTransferencia))
                ModelState.AddModelError("NumeroTransferencia", "Debe ingresar el número de transferencia.");

            if (viewModel.FormaPago == "Cheque" && string.IsNullOrWhiteSpace(viewModel.NumeroCheque))
                ModelState.AddModelError("NumeroCheque", "Debe ingresar el número de cheque.");

            if (viewModel.FormaPago == "Débito" && string.IsNullOrWhiteSpace(viewModel.NumeroTransaccion))
                ModelState.AddModelError("NumeroTransaccion", "Debe ingresar el número de transacción.");

            // Validar duplicados y números válidos
            if (!string.IsNullOrWhiteSpace(viewModel.NumeroTransferencia))
            {
                if (!long.TryParse(viewModel.NumeroTransferencia, out _))
                    ModelState.AddModelError("NumeroTransferencia", "Debe ser un número válido.");
                else if (db.PagosProveedores.Any(p => p.NumeroTransferencia == viewModel.NumeroTransferencia && p.IdPagoProveedor != viewModel.IdPagoProveedor))
                    ModelState.AddModelError("NumeroTransferencia", "Este número de transferencia ya está registrado.");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.NumeroCheque))
            {
                if (!long.TryParse(viewModel.NumeroCheque, out _))
                    ModelState.AddModelError("NumeroCheque", "Debe ser un número válido.");
                else if (db.PagosProveedores.Any(p => p.NumeroCheque == viewModel.NumeroCheque && p.IdPagoProveedor != viewModel.IdPagoProveedor))
                    ModelState.AddModelError("NumeroCheque", "Este número de cheque ya está registrado.");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.NumeroTransaccion))
            {
                if (!long.TryParse(viewModel.NumeroTransaccion, out _))
                    ModelState.AddModelError("NumeroTransaccion", "Debe ser un número válido.");
                else if (db.PagosProveedores.Any(p => p.NumeroTransaccion == viewModel.NumeroTransaccion && p.IdPagoProveedor != viewModel.IdPagoProveedor))
                    ModelState.AddModelError("NumeroTransaccion", "Este número de transacción ya está registrado.");
            }

            // Si hay errores, recargar lista de proveedores
            if (!ModelState.IsValid)
            {
                ViewBag.IdProveedor = new SelectList(db.Proveedores, "IdProveedor", "Nombre", viewModel.IdProveedor);
                return View(viewModel);
            }

            // Guardar cambios
            pagoOriginal.FechaPago = viewModel.FechaPago;
            pagoOriginal.Monto = montoDecimal;
            pagoOriginal.FormaPago = viewModel.FormaPago;
            pagoOriginal.TipoPago = viewModel.TipoPago;
            pagoOriginal.NumeroTransferencia = string.IsNullOrWhiteSpace(viewModel.NumeroTransferencia) ? null : viewModel.NumeroTransferencia;
            pagoOriginal.NumeroCheque = string.IsNullOrWhiteSpace(viewModel.NumeroCheque) ? null : viewModel.NumeroCheque;
            pagoOriginal.NumeroTransaccion = string.IsNullOrWhiteSpace(viewModel.NumeroTransaccion) ? null : viewModel.NumeroTransaccion;
            pagoOriginal.Observaciones = viewModel.Observaciones;

            var comprobante = await db.ComprobantesProveedores
            .FirstOrDefaultAsync(c => c.IdPagoProveedor == pagoOriginal.IdPagoProveedor);


            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // GET: PagoProveedors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var pagoProveedor = await db.PagosProveedores
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.IdPagoProveedor == id);

            if (pagoProveedor == null)
                return HttpNotFound();

            return View(pagoProveedor);
        }

        // POST: PagoProveedors/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var pagoProveedor = await db.PagosProveedores
                    .Include(p => p.Proveedor)
                    .FirstOrDefaultAsync(p => p.IdPagoProveedor == id);

                if (pagoProveedor == null)
                    return HttpNotFound();

                // ⚡ Eliminar comprobante si existe
                var comprobante = await db.ComprobantesProveedores
                    .FirstOrDefaultAsync(c => c.IdPagoProveedor == pagoProveedor.IdPagoProveedor);

                if (comprobante != null)
                {
                    db.ComprobantesProveedores.Remove(comprobante);
                }

                // Eliminar pago
                db.PagosProveedores.Remove(pagoProveedor);
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // ⚡ Log del error para debugging
                System.Diagnostics.Debug.WriteLine($"Error al eliminar el pago de proveedor: {ex.Message}");

                // Mensaje para la vista
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el pago del proveedor. Inténtelo nuevamente.";
                return RedirectToAction("Index");
            }
        }


        public async Task<ActionResult> BuscarPagos(string query)
        {
            var pagos = db.PagosProveedores
                .Include(p => p.Proveedor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                pagos = pagos.Where(p =>
                    p.Proveedor.Nombre.Contains(query) ||
                    p.Proveedor.RUT.ToString().Contains(query) || 
                    p.FormaPago.Contains(query) ||
                    p.TipoPago.Contains(query) ||
                    p.Observaciones.Contains(query)
                );
            }

            var resultados = await pagos
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return PartialView("_PagosProveedoresParcial", resultados);
        }


        // GET: PagoProveedors/SeleccionarProveedorParaPago
        public async Task<ActionResult> SeleccionarProveedorParaPago()
        {
            var proveedores = await db.Proveedores.ToListAsync();
            return View(proveedores);
        }

        // GET: PagoProveedors/BuscarProveedorParaPago
        public async Task<ActionResult> BuscarProveedorParaPago(string query)
        {
            var proveedores = await db.Proveedores
                .Where(p => p.Nombre.Contains(query) ||
                            p.RUT.ToString().Contains(query) ||
                            p.Direccion.Contains(query))
                .ToListAsync();

            return PartialView("_ProveedoresParaPagoParcial", proveedores);
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
