using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TesisSGC;
using System.Data.Entity;

public class MiPerfilController : Controller
{
    private Context db = new Context();
    public ActionResult Index()
    {
        string nombreUsuario = Session["Usuario"].ToString();
        var usuario = db.Usuarios
            .Include(u => u.Socio.Cuenta.CuotasMensuales)
            .FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

        if (usuario == null || usuario.Socio == null)
        {
            return HttpNotFound("No se encontró el socio.");
        }

        // Traer pagos del socio
        var pagos = db.PagosSocios
              .Include(p => p.CuotaMensual.UR)
              .Where(p => p.IdSocio == usuario.Socio.IdSocio)
              .OrderByDescending(p => p.FechaPago)
              .ToList();

        // ✅ CORRECCIÓN: Extraer IDs primero
        var pagoIds = pagos.Select(p => p.IdPagoSocio).ToList();

        // Traer comprobantes usando los IDs extraídos
        var comprobantes = db.Comprobantes
                             .Where(c => pagoIds.Contains(c.IdPagoSocio))
                             .ToList();

        ViewBag.PagosSocio = pagos;
        ViewBag.Comprobantes = comprobantes;
        return View(usuario.Socio);
    }
}
