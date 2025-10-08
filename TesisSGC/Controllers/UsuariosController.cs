using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TesisSGC.Helpers;

namespace TesisSGC
{
    public class UsuariosController : Controller
    {
        private Context db = new Context();

        // GET: Usuarios
        public async Task<ActionResult> Index()
        {
            var usuarios = db.Usuarios.Include(u => u.Socio);
            return View(await usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUsuario,NombreUsuario,Contrasena,Rol,Email,IdSocio")] Usuario usuario)
        {
            // 1. Si el rol es Socio, debe tener socio asociado
            if (usuario.Rol == "socio" && usuario.IdSocio == null)
            {
                ModelState.AddModelError("IdSocio", "El rol 'Socio' requiere un socio asociado.");
            }

            // 2. Validar si el socio ya tiene usuario asignado
            if (usuario.IdSocio != null)
            {
                bool socioYaTieneUsuario = db.Usuarios.Any(u => u.IdSocio == usuario.IdSocio);
                if (socioYaTieneUsuario)
                {
                    ModelState.AddModelError("IdSocio", "Este socio ya tiene un usuario asignado.");
                }
            }

            // 3. Validar si el nombre de usuario ya existe
            bool nombreUsuarioExiste = db.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario);
            if (nombreUsuarioExiste)
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está en uso.");
            }

            if (ModelState.IsValid)
            {
                // 🔐 Hashear la contraseña antes de guardar
                usuario.Contrasena = PasswordHelper.HashPassword(usuario.Contrasena);

                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", usuario.IdSocio);
            return View(usuario);
        }



        // GET: Usuarios/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", usuario.IdSocio);
            return View(usuario);
        }

        // POST: Usuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdUsuario,NombreUsuario,Contrasena,Rol,Email,IdSocio")] Usuario usuario)
        {
            // 1. Validación obligatoria: Rol socio requiere socio asociado
            if (usuario.Rol == "socio" && usuario.IdSocio == null)
            {
                ModelState.AddModelError("IdSocio", "El rol 'Socio' requiere un socio asociado.");
            }

            // 2. Validación de duplicación de socio
            bool socioAsignado = usuario.IdSocio != null &&
                db.Usuarios.Any(u => u.IdUsuario != usuario.IdUsuario && u.IdSocio == usuario.IdSocio);
            if (socioAsignado)
            {
                ModelState.AddModelError("IdSocio", "Ya existe un usuario asociado a ese socio.");
            }

            // 3. Validación de nombre de usuario duplicado
            bool nombreUsuarioRepetido = db.Usuarios.Any(u => u.IdUsuario != usuario.IdUsuario && u.NombreUsuario == usuario.NombreUsuario);
            if (nombreUsuarioRepetido)
            {
                ModelState.AddModelError("NombreUsuario", "El nombre de usuario ya está en uso.");
            }

            // Si todo está bien, guardar
            if (ModelState.IsValid)
            {
                db.Entry(usuario).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.IdSocio = new SelectList(db.Socios, "IdSocio", "NombreSocio", usuario.IdSocio);
            return View(usuario);
        }


        // GET: Usuarios/Delete
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Usuario usuarioAEliminar = await db.Usuarios.FindAsync(id);
            string usuarioActual = Session["Usuario"] as string;

            if (usuarioAEliminar != null && usuarioAEliminar.NombreUsuario == usuarioActual)
            {
                TempData["ErrorEliminar"] = "No puedes eliminar tu propio usuario mientras estás logueado.";
                return RedirectToAction("Index");
            }

            db.Usuarios.Remove(usuarioAEliminar);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> BuscarUsuarios(string query)
        {
            var usuarios = db.Usuarios.Include(u => u.Socio).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                usuarios = usuarios.Where(u =>
                    u.NombreUsuario.Contains(query) ||
                    (u.IdSocio != null && u.Socio.NombreSocio.Contains(query))
                );
            }

            return PartialView("_UsuariosParcial", await usuarios.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasena(int IdUsuario, string NuevaContrasena, string RepetirContrasena)
        {
            if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena != RepetirContrasena)
            {
                TempData["Error"] = "Las contraseñas no coinciden o están vacías.";
                return RedirectToAction("Edit", new { id = IdUsuario });
            }

            var usuario = await db.Usuarios.FindAsync(IdUsuario);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            usuario.Contrasena = PasswordHelper.HashPassword(NuevaContrasena);
            await db.SaveChangesAsync();

            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Edit", new { id = IdUsuario });
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
