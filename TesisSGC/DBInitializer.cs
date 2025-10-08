using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using TesisSGC.Helpers;

namespace TesisSGC
{
    public class DBInitializer : CreateDatabaseIfNotExists<Context>
    {
        protected override void Seed(Context context)
        {
            if (!context.Usuarios.Any(u => u.Rol == "administrador"))
            {
                var admin = new Usuario
                {
                    NombreUsuario = "administrador",
                    Contrasena = PasswordHelper.HashPassword("123456"),
                    Rol = "administrador"
                };

                context.Usuarios.Add(admin);
                context.SaveChanges();
            }

            base.Seed(context);
        }

    }
}
