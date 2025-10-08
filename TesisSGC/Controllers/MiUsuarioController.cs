using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TesisSGC.Helpers;

namespace TesisSGC.Controllers
{
    public class MiUsuarioController : Controller
    {
        private Context db = new Context();

        // GET: MiUsuario/Index
        public async Task<ActionResult> Index()
        {
            string nombreUsuario = Session["Usuario"] as string;
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return RedirectToAction("Login", "Auth");
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            return View(usuario); // Vista: Index.cshtml
        }

        // POST: MiUsuario/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index([Bind(Include = "IdUsuario,NombreUsuario,Email")] Usuario usuario)
        {
            var original = await db.Usuarios.FindAsync(usuario.IdUsuario);
            if (original == null)
            {
                return HttpNotFound();
            }

            // Validar nombre duplicado
            bool nombreDuplicado = db.Usuarios.Any(u => u.IdUsuario != usuario.IdUsuario && u.NombreUsuario == usuario.NombreUsuario);
            if (nombreDuplicado)
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está en uso.");
            }

            ModelState.Remove("Rol");
            ModelState.Remove("Contrasena");

            if (ModelState.IsValid)
            {
                original.NombreUsuario = usuario.NombreUsuario;
                original.Email = usuario.Email;

                await db.SaveChangesAsync();

                Session["Usuario"] = original.NombreUsuario;

                TempData["Mensaje"] = "Datos actualizados correctamente.";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        // POST: MiUsuario/CambiarContrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasena(int IdUsuario, string NuevaContrasena, string RepetirContrasena)
        {
            if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena != RepetirContrasena)
            {
                TempData["Error"] = "Las contraseñas no coinciden o están vacías.";
                return RedirectToAction("Index");
            }

            var usuario = await db.Usuarios.FindAsync(IdUsuario);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            usuario.Contrasena = PasswordHelper.HashPassword(NuevaContrasena);
            await db.SaveChangesAsync();

            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index");
        }
    }
}
