using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TesisSGC
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string rol = Session["Rol"] as string;

                if (rol == "administrador")
                {
                    Socios.Visible = false;
                    Administracion.Visible = true;
                    Proveedores.Visible = true;
                    Caja.Visible = true;
                    Reporte.Visible = true;
                }
                else
                {
                    Socios.Visible = true;
                    Administracion.Visible = false;
                    Proveedores.Visible = false;
                    Caja.Visible = false;
                    Reporte.Visible = false;
                }
            }
        }


    }
    }
    
