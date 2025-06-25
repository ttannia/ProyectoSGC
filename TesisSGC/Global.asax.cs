using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace TesisSGC
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Inicializador de Entity Framework
            Database.SetInitializer(new DBInitializer());

            // Configuración de rutas y bundles
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Forzar creación de base de datos si no existe
            using (var context = new Context())
            {
                var dummy = context.Socios.FirstOrDefault();
            }
        }
    }
}
