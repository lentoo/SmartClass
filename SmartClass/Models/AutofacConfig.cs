using Autofac;
using Autofac.Integration.Mvc;
using Common;
using System.Reflection;
using System.Web.Mvc;

namespace SmartClass.Models
{
    public class AutofacConfig
    {
        public static void init()
        {
            #region Autofac配置
            ContainerBuilder builder = new ContainerBuilder();
            var service = Assembly.Load("BLL");
            builder.RegisterAssemblyTypes(service).AsImplementedInterfaces();
            //builder.RegisterType<MyUserInfoService>().Named<IUserInfoService>("My");
            //builder.RegisterType<ILogHelper>().Named<NLogHelper>("NLog");
            var common = Assembly.Load("Common");
            builder.RegisterAssemblyTypes(common).AsImplementedInterfaces();
            //属性注入
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            //构造函数注入
            //builder.RegisterControllers(Assembly.GetExecutingAssembly())
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion
        }
    }
}