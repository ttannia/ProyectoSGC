using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TesisSGC.Helpers;

namespace TesisSGC.Controllers
{
    public class LoginController : Controller
    {
        // GET: /Login
        public ActionResult Login()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(ViewModels.LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var context = new Context())
                {
                    // Busca el usuario en la base de datos
                    var usuario = context.Usuarios
                                         .FirstOrDefault(u => u.NombreUsuario == model.Username);

                    if (usuario != null)
                    {
                        // Verifica la contraseña
                        bool esValida = PasswordHelper.VerifyPassword(model.Password, usuario.Contrasena);

                        if (esValida)
                        {
                            Session["UsuarioID"] = usuario.IdUsuario;
                            Session["Usuario"] = usuario.NombreUsuario;
                            Session["Rol"] = usuario.Rol ?? "";

                            return Redirect("~/Default.aspx");
                        }
                    }

                    // Si llega acá, usuario o contraseña incorrectos
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(model);
            }
        }


        public ActionResult Logout()
        {
            // Limpiar sesión
            Session.Clear();
            Session.Abandon();

            // Borrar cookie de autenticación
            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                var cookie = new HttpCookie(".ASPXAUTH");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            // Redirigir al Login MVC
            return RedirectToAction("Login", "Login");

        }

    }

}

