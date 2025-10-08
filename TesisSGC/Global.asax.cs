using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Mvc;

namespace TesisSGC
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Inicializador de Entity Framework
            Database.SetInitializer(new DBInitializer());

            // Configuración de rutas y bundles
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Forzar creación de base de datos si no existe
            using (var context = new Context())
            {
                var dummy = context.Socios.FirstOrDefault();
            }
        }

        //para lo del cache
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            // Forzar cultura española
            var culture = new System.Globalization.CultureInfo("es-AR");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            // Cache
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
        }

    }
}
