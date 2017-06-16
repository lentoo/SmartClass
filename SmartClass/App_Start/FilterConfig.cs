using SmartClass.Models.Exceptions;
using SmartClass.Models;
using System.Web;
using System.Web.Mvc;

namespace SmartClass
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyException());
            filters.Add(new MyActionFilterAttribute());
        }
    }
}
