using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration;
using SmartClass.Models;
using SmartClass.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SmartClass
{
    public class MvcApplication : HttpApplication
    {
       
        protected void Application_Start()
        {        
            //autofac配置
            AutofacConfig.Init();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);      
               
            //扫描异常信息
            MyException.ProcessException();
        }
        
    }
}
