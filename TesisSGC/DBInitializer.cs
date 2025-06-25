using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TesisSGC
{
    public class DBInitializer : CreateDatabaseIfNotExists<Context>
    {
        protected override void Seed(Context context)
        {
            // Opcional: podés cargar datos iniciales aquí
            base.Seed(context);
        }
    }
}