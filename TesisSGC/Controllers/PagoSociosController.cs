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
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;
using System.Dynamic;
using TesisSGC.ViewModels;
using System.Globalization;


namespace TesisSGC
{
    public class PagoSociosController : Controller
    {
        private Context db = new Context();

        private string CapitalizeFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        // GET: PagoSocios
        public async Task<ActionResult> Index()
        {
            var pagosSocios = db.PagosSocios
                .Include(p => p.CuotaMensual)
                .Include(p => p.Socio)
                .OrderByDescending(p => p.FechaPago); 

            return View(await pagosSocios.ToListAsync());
        }

        // GET: PagoSocios/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            try
            {
                var pago = await db.PagosSocios
                                   .Include(p => p.Socio)
                                   .Include(p => p.CuotaMensual)
                                   .FirstOrDefaultAsync(p => p.IdPagoSocio == id);

                if (pago == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el pago del socio.";
                    return RedirectToAction("Index");
                }

                var comprobante = await db.Comprobantes
                                  .FirstOrDefaultAsync(c => c.IdPagoSocio == pago.IdPagoSocio);

                ViewBag.ComprobanteId = comprobante?.IdComprobante;

                return View(pago);
            }
            catch (Exception ex)
            {
                // ⚡ Log para debugging
                System.Diagnostics.Debug.WriteLine($"Error al obtener detalles del pago del socio: {ex.Message}");

                TempData["ErrorMessage"] = "Ocurrió un error al obtener los detalles del pago. Inténtelo nuevamente.";
                return RedirectToAction("Index");
            }
        }


        // ✅ NUEVA ACCIÓN
        public ActionResult Comprobante(int id)
        {
            try
            {
                // Buscar comprobante con sus relaciones
                var comprobante = db.Comprobantes
                    .Include(c => c.PagoSocio)
                    .Include(c => c.PagoSocio.Socio)
                    .Include(c => c.PagoSocio.CuotaMensual)
                    .SingleOrDefault(c => c.IdComprobante == id);

                if (comprobante == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el comprobante solicitado.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Nombre de archivo PDF
                string fileName = $"Comprobante_{comprobante.NumeroComprobante}.pdf";

                return new Rotativa.ViewAsPdf("Comprobante", comprobante)
                {
                    FileName = fileName,
                    PageSize = Rotativa.Options.Size.A4,
                    PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10),
                    CustomSwitches = "--print-media-type --disable-smart-shrinking"
                };
            }
            catch (Exception ex)
            {
                // ⚡ Log interno para debugging
                System.Diagnostics.Debug.WriteLine($"Error al generar comprobante socio: {ex.Message}");

                TempData["ErrorMessage"] = "Ocurrió un error al generar el comprobante. Inténtelo nuevamente.";
                return RedirectToAction("Index");
            }
        }



        // GET: PagoSocios/Create
        public ActionResult Create(int? idSocio)
        {
            if (idSocio == null)
            {
                return RedirectToAction("SeleccionarSocioParaPago");
            }

            var socio = db.Socios.Find(idSocio);
            if (socio == null)
            {
                return HttpNotFound();
            }

            var cuotas = db.CuotasMensuales
            .Include(c => c.PagoSocios)  
            .Where(c => c.IdSocio == idSocio && !c.EstaPagada)
            .ToList();

            foreach (var c in cuotas)
            {
                decimal valorUR = db.URs
                    .Where(u => u.Mes.Year == c.Mes.Year && u.Mes.Month == c.Mes.Month)
                    .Select(u => (decimal?)u.Monto)
                    .FirstOrDefault() ?? 1m;

                var pagos = db.PagosSocios
                .Where(p => p.IdCuotaMensual == c.IdCuotaMensual)
                .ToList();

                decimal totalPagadoUR = pagos.Sum(p => p.Monto / valorUR);

                c.SaldoPendiente = c.Monto - totalPagadoUR;
            }

            // Formateamos Mes como "yyyy-MM-dd" para evitar problemas en JS
            var valoresUR = db.URs
            .AsEnumerable()
            .Select(u =>
            {
                dynamic obj = new ExpandoObject();
                obj.IdUR = u.IdUR;
                obj.Mes = u.Mes.ToString("yyyy-MM-dd");
                obj.Monto = u.Monto;
                return obj;
            })
            .ToList();


            var pago = new PagoSocio
            {
                IdSocio = idSocio.Value,
                FechaPago = DateTime.Now
            };

            ViewBag.NombreSocio = socio.NombreSocio;
            ViewBag.CI = socio.CI;
            ViewBag.Cuotas = cuotas;
            ViewBag.ValoresUR = valoresUR;

            return View(pago);
        }

        //POST: PagoSocios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdPagoSocio,IdSocio,IdCuotaMensual,FechaPago,Monto,MontoUR,FormaPago,TipoPago,NumeroTransferencia,NumeroCheque,NumeroTransaccion,Observaciones")] PagoSocio pagoSocio)
        {
            var cuota = db.CuotasMensuales.Find(pagoSocio.IdCuotaMensual);

            decimal valorURdelMes = 1m; // valor por defecto para evitar errores

            if (cuota != null)
            {
                // Obtener valor UR del mes, si existe
                var ur = db.URs.FirstOrDefault(u => u.Mes.Year == cuota.Mes.Year && u.Mes.Month == cuota.Mes.Month);
                if (ur != null)
                {
                    valorURdelMes = ur.Monto;
                }

                // Pagos existentes
                var pagos = db.PagosSocios
                    .Where(p => p.IdCuotaMensual == cuota.IdCuotaMensual)
                    .Select(p => (decimal?)p.Monto)
                    .ToList();

                decimal totalPagadoPesos = pagos.Sum(p => p ?? 0);
                decimal totalPagadoUR = totalPagadoPesos / valorURdelMes;

                decimal nuevoTotalPesos = totalPagadoPesos + pagoSocio.Monto;
                decimal nuevoSaldoUR = cuota.Monto - (nuevoTotalPesos / valorURdelMes);
                
                decimal saldoPendientePesos = cuota.Monto * valorURdelMes - totalPagadoPesos;

                if (pagoSocio.Monto <= 0)
                    ModelState.AddModelError("Monto", "El monto debe ser mayor a 0.");

                if (pagoSocio.Monto > saldoPendientePesos)
                    ModelState.AddModelError("Monto", $"El monto ingresado excede el saldo pendiente de ${saldoPendientePesos:F2}.");

                //No puede estar vacío 
                if (pagoSocio.FormaPago == "Transferencia")
                {
                    if (string.IsNullOrWhiteSpace(pagoSocio.NumeroTransferencia))
                        ModelState.AddModelError("NumeroTransferencia", "Debe ingresar el número de transferencia.");
                }

                if (pagoSocio.FormaPago == "Cheque")
                {
                    if (string.IsNullOrWhiteSpace(pagoSocio.NumeroCheque))
                        ModelState.AddModelError("NumeroCheque", "Debe ingresar el número de cheque.");
                }

                if (pagoSocio.FormaPago == "Débito")
                {
                    if (string.IsNullOrWhiteSpace(pagoSocio.NumeroTransaccion))
                        ModelState.AddModelError("NumeroTransaccion", "Debe ingresar el número de transacción.");
                }

                // Validar NumeroTransferencia único (solo si no está vacío)
                // Transferencia
                if (!string.IsNullOrWhiteSpace(pagoSocio.NumeroTransferencia))
                {
                    if (!long.TryParse(pagoSocio.NumeroTransferencia, out _))
                    {
                        ModelState.AddModelError("NumeroTransferencia", "Debe ser un número válido.");
                    }
                    else
                    {
                        bool transferenciaDuplicada = db.PagosSocios
                            .Any(p => p.NumeroTransferencia == pagoSocio.NumeroTransferencia);
                        if (transferenciaDuplicada)
                            ModelState.AddModelError("NumeroTransferencia", "Este número de transferencia ya está registrado.");
                    }
                }

                // Cheque
                if (!string.IsNullOrWhiteSpace(pagoSocio.NumeroCheque))
                {
                    if (!long.TryParse(pagoSocio.NumeroCheque, out _))
                    {
                        ModelState.AddModelError("NumeroCheque", "Debe ser un número válido.");
                    }
                    else
                    {
                        bool chequeDuplicado = db.PagosSocios
                            .Any(p => p.NumeroCheque == pagoSocio.NumeroCheque);
                        if (chequeDuplicado)
                            ModelState.AddModelError("NumeroCheque", "Este número de cheque ya está registrado.");
                    }
                }

                // Transacción
                if (!string.IsNullOrWhiteSpace(pagoSocio.NumeroTransaccion))
                {
                    if (!long.TryParse(pagoSocio.NumeroTransaccion, out _))
                    {
                        ModelState.AddModelError("NumeroTransaccion", "Debe ser un número válido.");
                    }
                    else
                    {
                        bool transaccionDuplicada = db.PagosSocios
                            .Any(p => p.NumeroTransaccion == pagoSocio.NumeroTransaccion);
                        if (transaccionDuplicada)
                            ModelState.AddModelError("NumeroTransaccion", "Este número de transacción ya está registrado.");
                    }
                }

            }
            else
            {
                ModelState.AddModelError("", "No se encontró la cuota seleccionada.");
            }

            // Solo agregamos si ModelState sigue siendo válido
            if (ModelState.IsValid && cuota != null)
            {
                pagoSocio.NumeroTransferencia = string.IsNullOrWhiteSpace(pagoSocio.NumeroTransferencia) ? null : pagoSocio.NumeroTransferencia;
                pagoSocio.NumeroCheque = string.IsNullOrWhiteSpace(pagoSocio.NumeroCheque) ? null : pagoSocio.NumeroCheque;
                pagoSocio.NumeroTransaccion = string.IsNullOrWhiteSpace(pagoSocio.NumeroTransaccion) ? null : pagoSocio.NumeroTransaccion;

                // Recalculamos el total pagado para actualizar saldo
                var pagos = db.PagosSocios
                    .Where(p => p.IdCuotaMensual == cuota.IdCuotaMensual)
                    .Select(p => (decimal?)p.Monto)
                    .ToList();

                decimal totalPagadoUR = pagos.Sum(p => (p ?? 0) / valorURdelMes);

                // Recalculamos saldo en UR
                decimal nuevoSaldoUR = cuota.Monto - (totalPagadoUR + (pagoSocio.Monto / valorURdelMes));
                cuota.SaldoPendiente = Math.Max(0, nuevoSaldoUR);
                cuota.EstaPagada = cuota.SaldoPendiente <= 0.001m;

                db.PagosSocios.Add(pagoSocio);
                await db.SaveChangesAsync();


                // Después de guardar el pago
                try
                {
                    var yaExiste = db.Comprobantes.FirstOrDefault(c => c.IdPagoSocio == pagoSocio.IdPagoSocio);
                    if (yaExiste == null)
                    {
                        int siguienteId = (db.Comprobantes.Any()) ? db.Comprobantes.Max(c => c.IdComprobante) + 1 : 1;
                        string numeroComprobante = $"C-{siguienteId:D5}";

                        var comprobante = new Comprobante
                        {
                            IdPagoSocio = pagoSocio.IdPagoSocio,
                            FechaEmision = DateTime.Now,
                            NumeroComprobante = numeroComprobante
                        };

                        db.Comprobantes.Add(comprobante);
                        await db.SaveChangesAsync();

                        return RedirectToAction("DescargarComprobanteYRedirigir", new { id = comprobante.IdComprobante });
                    }
                    else
                    {
                        return RedirectToAction("DescargarComprobanteYRedirigir", new { id = yaExiste.IdComprobante });
                    }
                }
                catch (Exception ex)
                {
                    // ⚡ Log del error
                    System.Diagnostics.Debug.WriteLine($"Error al generar comprobante: {ex.Message}");

                    // Redirigimos a detalles del pago para que el admin pueda verlo aunque no tenga comprobante
                    TempData["ErrorMessage"] = "El pago fue registrado, pero no se pudo generar el comprobante.";
                    return RedirectToAction("Details", new { id = pagoSocio.IdPagoSocio });
                }

            }

            // Si falla validación, recargar ViewBag como antes
            var socio = db.Socios.Find(pagoSocio.IdSocio);
            ViewBag.NombreSocio = socio?.NombreSocio ?? "";
            ViewBag.CI = socio?.CI.ToString() ?? "";

            ViewBag.Cuotas = db.CuotasMensuales
                .Include("PagoSocios")
                .Where(c => c.IdSocio == pagoSocio.IdSocio)
                .ToList();

            ViewBag.ValoresUR = db.URs
                .AsEnumerable()
                .Select(u =>
                {
                    dynamic obj = new ExpandoObject();
                    obj.IdUR = u.IdUR;
                    obj.Mes = u.Mes.ToString("yyyy-MM-dd");
                    obj.Monto = u.Monto;
                    return obj;
                })
                .ToList();

            return View(pagoSocio);
        }

        // ✅ NUEVA ACCIÓN PARA MANEJAR DESCARGA Y REDIRECCIÓN
        public ActionResult DescargarComprobanteYRedirigir(int id)
        {
            // Pasamos el ID del comprobante a la vista
            ViewBag.ComprobanteId = id;
            return View();
        }


        // GET: PagoSocios/SeleccionarSocioParaPago
        public async Task<ActionResult> SeleccionarSocioParaPago()
        {
            var socios = await db.Socios.ToListAsync();
            return View(socios);
        }

        public async Task<ActionResult> BuscarSocioParaPago(string query)
        {
            var socios = await db.Socios
                .Where(s => s.NombreSocio.Contains(query) ||
                            SqlFunctions.StringConvert((double)s.CI).Trim().Contains(query))
                .ToListAsync();

            return PartialView("_SociosParaPagoParcial", socios);
        }


        // GET: PagoSocios/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var pago = await db.PagosSocios
                .Include(p => p.CuotaMensual)
                .Include(p => p.Socio)
                .FirstOrDefaultAsync(p => p.IdPagoSocio == id);

            if (pago == null)
                return HttpNotFound();

            var cuota = pago.CuotaMensual;
            decimal valorURdelMes = 1m;
            decimal saldoURSinEste = 0m; // <-- declarar antes

            if (cuota != null)
            {
                var ur = await db.URs
                    .FirstOrDefaultAsync(u => u.Mes.Year == cuota.Mes.Year && u.Mes.Month == cuota.Mes.Month);

                if (ur != null)
                    valorURdelMes = ur.Monto;

                // Suma de pagos de la cuota EXCLUYENDO el pago actual (en pesos)
                var pagosOtrosPesos = await db.PagosSocios
                    .Where(pp => pp.IdCuotaMensual == cuota.IdCuotaMensual && pp.IdPagoSocio != pago.IdPagoSocio)
                    .Select(pp => (decimal?)pp.Monto)
                    .ToListAsync();

                decimal totalOtrosPesos = pagosOtrosPesos.Sum(x => x ?? 0m);

                // Saldo pendiente en UR excluyendo este pago
                saldoURSinEste = cuota.Monto - (totalOtrosPesos / valorURdelMes);
                if (saldoURSinEste < 0) saldoURSinEste = 0;

                ViewBag.SaldoPendienteURSinEste = saldoURSinEste; // útil para mostrar en la vista
                ViewBag.ValorUR = valorURdelMes;
            }

            // Datos de cabecera
            ViewBag.NombreSocio = pago?.Socio?.NombreSocio ?? "";
            ViewBag.CI = pago?.Socio?.CI.ToString() ?? "";

            // Si tu vista usa la misma lógica JS que Create:
            ViewBag.ValoresUR = db.URs
                .AsEnumerable()
                .Select(u =>
                {
                    dynamic obj = new ExpandoObject();
                    obj.IdUR = u.IdUR;
                    obj.Mes = u.Mes.ToString("yyyy-MM-dd");
                    obj.Monto = u.Monto;
                    return obj;
                })
                .ToList();

            var viewModel = new PagoSocioEditViewModel
            {
                IdPagoSocio = pago.IdPagoSocio,
                IdSocio = pago.IdSocio,
                IdCuotaMensual = pago.IdCuotaMensual,
                FechaPago = pago.FechaPago,
                Monto = pago.Monto.ToString("0.##", CultureInfo.InvariantCulture),
                FormaPago = pago.FormaPago,
                TipoPago = pago.TipoPago,
                NumeroTransferencia = pago.NumeroTransferencia,
                NumeroCheque = pago.NumeroCheque,
                NumeroTransaccion = pago.NumeroTransaccion,
                Observaciones = pago.Observaciones,
                NombreSocio = pago.Socio?.NombreSocio,
                CI = pago.Socio?.CI.ToString(),
                SaldoPendienteURSinEste = saldoURSinEste,
                ValorUR = valorURdelMes,
                MontoCuota = pago.CuotaMensual.Monto,

                // ⚡ Datos de la cuota
                MesCuota = pago.CuotaMensual.Mes,
                TipoCuota = pago.CuotaMensual.Tipo
            };

            return View(viewModel);

        }


        // POST: PagoSocios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PagoSocioEditViewModel viewModel)
        {
            // ✅ CAMBIO: Usar el método del ViewModel para validar el monto
            decimal montoDecimal = 0;
            if (!viewModel.TryGetMontoDecimal(out montoDecimal) || montoDecimal <= 0)
            {
                ModelState.AddModelError("Monto", "El Monto debe ser un número válido mayor a 0 (ej: 123 o 123,45).");
            }

            // Traer el pago original con sus relaciones
            var pagoOriginal = await db.PagosSocios
                .Include(p => p.CuotaMensual)
                .Include(p => p.Socio)
                .FirstOrDefaultAsync(p => p.IdPagoSocio == viewModel.IdPagoSocio);

            if (pagoOriginal == null)
                return HttpNotFound();

            var cuota = pagoOriginal.CuotaMensual;
            if (cuota == null)
                ModelState.AddModelError("", "No se encontró la cuota asociada al pago.");

            // Validaciones de negocio
            decimal valorURdelMes = 1m;
            decimal saldoPesosSinEste = 0m;

            if (cuota != null)
            {
                var ur = await db.URs
                    .FirstOrDefaultAsync(u => u.Mes.Year == cuota.Mes.Year && u.Mes.Month == cuota.Mes.Month);
                if (ur != null)
                    valorURdelMes = ur.Monto;

                // Suma de pagos de la misma cuota EXCLUYENDO este pago (en pesos)
                var pagosOtrosPesos = await db.PagosSocios
                    .Where(pp => pp.IdCuotaMensual == cuota.IdCuotaMensual && pp.IdPagoSocio != pagoOriginal.IdPagoSocio)
                    .Select(pp => (decimal?)pp.Monto)
                    .ToListAsync();

                decimal totalOtrosPesos = pagosOtrosPesos.Sum(x => x ?? 0m);

                // Saldo pendiente en UR sin contar este pago
                decimal saldoURSinEste = cuota.Monto - (totalOtrosPesos / valorURdelMes);
                if (saldoURSinEste < 0) saldoURSinEste = 0;

                // Saldo pendiente en PESOS sin contar este pago (umbral para validar el nuevo monto)
                saldoPesosSinEste = saldoURSinEste * valorURdelMes;

                // Validación: monto no puede superar el saldo pendiente
                if (montoDecimal - saldoPesosSinEste > 0.01m)
                    ModelState.AddModelError("Monto", $"El monto ingresado excede el saldo pendiente de ${saldoPesosSinEste:F2}.");
            }

            // Validar que no estén vacíos según FormaPago
            if (viewModel.FormaPago == "Transferencia" && string.IsNullOrWhiteSpace(viewModel.NumeroTransferencia))
            {
                ModelState.AddModelError("NumeroTransferencia", "El número de transferencia es obligatorio.");
            }

            if (viewModel.FormaPago == "Cheque" && string.IsNullOrWhiteSpace(viewModel.NumeroCheque))
            {
                ModelState.AddModelError("NumeroCheque", "El número de cheque es obligatorio.");
            }

            if (viewModel.FormaPago == "Débito" && string.IsNullOrWhiteSpace(viewModel.NumeroTransaccion))
            {
                ModelState.AddModelError("NumeroTransaccion", "El número de transacción es obligatorio.");
            }

            // Validar NumeroTransferencia único (solo si no está vacío y cambió)
            if (!string.IsNullOrEmpty(viewModel.NumeroTransferencia)
                && viewModel.NumeroTransferencia != pagoOriginal.NumeroTransferencia)
            {
                bool transferenciaDuplicada = db.PagosSocios
                    .Any(p => p.NumeroTransferencia == viewModel.NumeroTransferencia
                              && p.IdPagoSocio != pagoOriginal.IdPagoSocio);
                if (transferenciaDuplicada)
                    ModelState.AddModelError("NumeroTransferencia", "Este número de transferencia ya está registrado.");
            }

            // Validar NumeroCheque único
            if (!string.IsNullOrEmpty(viewModel.NumeroCheque)
                && viewModel.NumeroCheque != pagoOriginal.NumeroCheque)
            {
                bool chequeDuplicado = db.PagosSocios
                    .Any(p => p.NumeroCheque == viewModel.NumeroCheque
                              && p.IdPagoSocio != pagoOriginal.IdPagoSocio);
                if (chequeDuplicado)
                    ModelState.AddModelError("NumeroCheque", "Este número de cheque ya está registrado.");
            }

            // Validar NumeroTransaccion único
            if (!string.IsNullOrEmpty(viewModel.NumeroTransaccion)
                && viewModel.NumeroTransaccion != pagoOriginal.NumeroTransaccion)
            {
                bool transaccionDuplicada = db.PagosSocios
                    .Any(p => p.NumeroTransaccion == viewModel.NumeroTransaccion
                              && p.IdPagoSocio != pagoOriginal.IdPagoSocio);
                if (transaccionDuplicada)
                    ModelState.AddModelError("NumeroTransaccion", "Este número de transacción ya está registrado.");
            }


            if (!ModelState.IsValid)
            {
                // Cabecera
                ViewBag.NombreSocio = pagoOriginal?.Socio?.NombreSocio ?? "";
                ViewBag.CI = pagoOriginal?.Socio?.CI.ToString() ?? "";

                // Recalcular info auxiliar para la vista
                decimal valorUR = 1m;

                if (cuota != null)
                {
                    var ur = await db.URs.FirstOrDefaultAsync(u => u.Mes.Year == cuota.Mes.Year && u.Mes.Month == cuota.Mes.Month);
                    if (ur != null) valorUR = ur.Monto;

                    var pagosOtrosPesos = await db.PagosSocios
                        .Where(pp => pp.IdCuotaMensual == cuota.IdCuotaMensual && pp.IdPagoSocio != pagoOriginal.IdPagoSocio)
                        .Select(pp => (decimal?)pp.Monto)
                        .ToListAsync();

                    decimal totalOtrosPesos = pagosOtrosPesos.Sum(x => x ?? 0m);
                    decimal saldoURSinEste = cuota.Monto - (totalOtrosPesos / valorUR);
                    if (saldoURSinEste < 0) saldoURSinEste = 0;

                    ViewBag.SaldoPendienteURSinEste = saldoURSinEste;
                    ViewBag.ValorUR = valorUR;
                }

                ViewBag.ValoresUR = db.URs
                    .AsEnumerable()
                    .Select(u =>
                    {
                        dynamic obj = new ExpandoObject();
                        obj.IdUR = u.IdUR;
                        obj.Mes = u.Mes.ToString("yyyy-MM-dd");
                        obj.Monto = u.Monto;
                        return obj;
                    })
                    .ToList();

                return View(viewModel);
            }

            // ===== Guardado =====
            pagoOriginal.FechaPago = viewModel.FechaPago;
            pagoOriginal.Monto = montoDecimal; // ✅ usar el decimal parseado correctamente
            pagoOriginal.FormaPago = viewModel.FormaPago;
            pagoOriginal.TipoPago = viewModel.TipoPago;
            pagoOriginal.NumeroTransferencia = string.IsNullOrWhiteSpace(viewModel.NumeroTransferencia) ? null : viewModel.NumeroTransferencia;
            pagoOriginal.NumeroCheque = string.IsNullOrWhiteSpace(viewModel.NumeroCheque) ? null : viewModel.NumeroCheque;
            pagoOriginal.NumeroTransaccion = string.IsNullOrWhiteSpace(viewModel.NumeroTransaccion) ? null : viewModel.NumeroTransaccion;
            pagoOriginal.Observaciones = viewModel.Observaciones;

            // Recalcular saldo de la cuota en UR (sumando todos los pagos con el nuevo monto)
            var pagosTodosPesos = await db.PagosSocios
                .Where(pp => pp.IdCuotaMensual == cuota.IdCuotaMensual && pp.IdPagoSocio != pagoOriginal.IdPagoSocio)
                .Select(pp => (decimal?)pp.Monto)
                .ToListAsync();

            decimal totalOtrosPesos2 = pagosTodosPesos.Sum(x => x ?? 0m);
            decimal nuevoSaldoUR = cuota.Monto - ((totalOtrosPesos2 + pagoOriginal.Monto) / valorURdelMes);
            if (nuevoSaldoUR < 0) nuevoSaldoUR = 0;

            cuota.SaldoPendiente = nuevoSaldoUR;
            cuota.EstaPagada = cuota.SaldoPendiente <= 0.001m;

            var comprobante = await db.Comprobantes
            .FirstOrDefaultAsync(c => c.IdPagoSocio == pagoOriginal.IdPagoSocio);

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // GET: PagoSocios/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pagoSocio = await db.PagosSocios
                                    .Include(p => p.CuotaMensual) // incluimos la cuota
                                    .FirstOrDefaultAsync(p => p.IdPagoSocio == id);

            if (pagoSocio == null)
            {
                return HttpNotFound();
            }

            return View(pagoSocio);
        }

        // POST: PagoSocios/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Traemos el pago a eliminar, incluyendo la cuota y el socio
                var pagoSocio = await db.PagosSocios
                                        .Include(p => p.CuotaMensual)
                                        .Include(p => p.Socio)
                                        .FirstOrDefaultAsync(p => p.IdPagoSocio == id);

                if (pagoSocio == null)
                    return HttpNotFound();

                var cuota = pagoSocio.CuotaMensual;

                // ⚡ Eliminar primero el comprobante asociado, si existe
                var comprobante = await db.Comprobantes
                                          .FirstOrDefaultAsync(c => c.IdPagoSocio == pagoSocio.IdPagoSocio);
                if (comprobante != null)
                {
                    db.Comprobantes.Remove(comprobante);
                    await db.SaveChangesAsync();
                }

                // Eliminamos el pago
                db.PagosSocios.Remove(pagoSocio);
                await db.SaveChangesAsync(); // ⚡ guardamos primero para que el pago desaparezca

                // Recalculamos todos los pagos restantes de esta cuota
                var pagosRestantes = await db.PagosSocios
                                              .Where(p => p.IdCuotaMensual == cuota.IdCuotaMensual)
                                              .ToListAsync();

                // Traer valor actual de la UR
                var valorUR = db.URs
                                .OrderByDescending(u => u.Mes)
                                .Select(u => (decimal?)u.Monto)
                                .FirstOrDefault() ?? 1400m;

                // Convertir cuota a pesos
                decimal montoCuotaEnPesos = cuota.Monto * valorUR;

                // Total pagado ya está en pesos
                decimal totalPagadoEnPesos = pagosRestantes.Sum(p => p.Monto);

                // ⚡ Recalcular saldo pendiente y estado
                cuota.SaldoPendiente = Math.Max(montoCuotaEnPesos - totalPagadoEnPesos, 0m);
                cuota.EstaPagada = totalPagadoEnPesos >= montoCuotaEnPesos;

                db.Entry(cuota).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // ⚡ Loguear el error (si usás logging, acá lo mandarías a un logger)
                System.Diagnostics.Debug.WriteLine($"Error al eliminar el pago: {ex.Message}");

                // Mostrar mensaje de error en TempData y redirigir al listado
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar el pago. Inténtelo nuevamente.";
                return RedirectToAction("Index");
            }
        }


        public async Task<ActionResult> BuscarPagos(string query)
        {
            var pagos = db.PagosSocios
                .Include(p => p.Socio)
                .Include(p => p.CuotaMensual)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                // Buscar por nombre de socio, CI, tipo de cuota, forma de pago, tipo de pago
                pagos = pagos.Where(p =>
                    p.Socio.NombreSocio.Contains(query) ||
                    SqlFunctions.StringConvert((double)p.Socio.CI).Trim().Contains(query) ||
                    p.CuotaMensual.Tipo.Contains(query) ||
                    p.FormaPago.Contains(query) ||
                    p.TipoPago.Contains(query) ||
                    p.Observaciones.Contains(query)
                );
            }

            var resultados = await pagos
                .OrderByDescending(p => p.FechaPago) // del más reciente al más antiguo
                .ToListAsync();

            return PartialView("_PagosParcial", resultados);
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
