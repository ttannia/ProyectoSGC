using System.Web;
using System.Web.Mvc;

namespace TesisSGC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeSessionAttribute()); // para todos
        }
    }
}
