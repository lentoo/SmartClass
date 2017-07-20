using Autofac;
using Autofac.Integration.Mvc;
using Common.Cache;
using System.Reflection;
using System.Web.Mvc;
using Model;

namespace SmartClass.Models
{
    public class AutofacConfig
    {
        public static void Init()
        {
            #region Autofac配置
            ContainerBuilder builder = new ContainerBuilder();
            var service = Assembly.Load("BLL");
            builder.RegisterTypes(service.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();
            var dal = Assembly.Load("DAL");
            builder.RegisterTypes(dal.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();
            var common = Assembly.Load("Common");
            builder.RegisterTypes(common.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();           
            builder.RegisterType<RedisWrite>().Named<ICacheHelper>("Redis");
            builder.RegisterType<MemcacheHelper>().Named<ICacheHelper>("Memcached");

            //builder.RegisterType<NFineBaseEntities>().InstancePerLifetimeScope().PropertiesAutowired();//单例模式注入

            //  builder.RegisterType<MyActionFilterAttribute>().PropertiesAutowired();
            //builder.RegisterType<MemcacheHelper>().As<ICacheHelper>().InstancePerLifetimeScope();   
            //var assemblys = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToList();
            //builder.RegisterAssemblyTypes(assemblys.ToArray()).Where(t => t.Name.EndsWith("Service")).AsImplementedInterfaces().PropertiesAutowired();

            //builder.RegisterAssemblyTypes(assemblys.ToArray())//查找程序集中以Dal结尾的类型  
            //.Where(t => t.Name.EndsWith("Dal"))
            //.AsImplementedInterfaces().PropertiesAutowired();//表示注册的类型，以接口的方式注册  
            //var IServices = Assembly.Load("IBLL");
            //var Services = Assembly.Load("BLL");
            //var IRepository = Assembly.Load("IDAL");
            //var Repository = Assembly.Load("DAL");
            //builder.RegisterAssemblyTypes(IServices, Services).AsImplementedInterfaces().PropertiesAutowired();
            //builder.RegisterAssemblyTypes(IRepository, Repository).AsImplementedInterfaces().PropertiesAutowired();

            //builder.RegisterType<UserInfoService>().Named<IUserInfoService>("My");
            //builder.RegisterType<NLogHelper>().As<ILogHelper>();//.InstancePerLifetimeScope();

            //属性注入
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            builder.RegisterFilterProvider();//.PropertiesAutowired();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            #endregion
        }
    }
}