using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TesisSGC
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Bloqueo de caché
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate, no-store, no-cache");

            // Redirigir si no hay sesión
            if (Session["UsuarioID"] == null)
            {
                Response.Redirect("~/Login/Login");
                return;
            }

            if (!IsPostBack)
            {
                // Validación de rol
                string rol = Session["Rol"] as string;

                if (rol == "socio")
                {
                    Response.Redirect("~/About.aspx");
                    return;
                }
            }
        }



    }


}
    
