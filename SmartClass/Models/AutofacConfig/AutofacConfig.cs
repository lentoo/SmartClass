using System.Reflection;
using System.Web.Mvc;

using Autofac.Extras.Quartz;

using SmartClass.Infrastructure.Cache;
using SmartClass.Models.Job;
using SmartClass.Models.SignalR;
using Microsoft.AspNet.SignalR;
using Autofac.Integration.SignalR;
using Autofac;
using Autofac.Integration.Mvc;

namespace SmartClass.Models.AutofacConfig
{
    public class AutofacConfig
    {
        private static IContainer AutofacContainer=null;

        public static IContainer GetContainer()
        {
            return AutofacContainer;
        }
        public static void Init()
        {
            #region Autofac配置
            ContainerBuilder builder = new ContainerBuilder();

            var service = Assembly.Load("SmartClass.Service");
            builder.RegisterTypes(service.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();
            var dal = Assembly.Load("SmartClass.Repository");
            builder.RegisterTypes(dal.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();
            var common = Assembly.Load("SmartClass.Infrastructure");
            builder.RegisterTypes(common.GetTypes()).AsImplementedInterfaces().PropertiesAutowired();

            builder.RegisterType<RedisWrite>().Named<ICacheHelper>("Redis");
            builder.RegisterType<MemcacheHelper>().Named<ICacheHelper>("Memcached");
            
            builder.RegisterType<ServiceGetter>().As<IServiceGetter>();
            builder.RegisterType<SearchService>().AsSelf();
            builder.RegisterType<SerialPortService>().AsSelf().PropertiesAutowired();
            //注册过滤器
            builder.RegisterFilterProvider();
            //Quartz注册定时任务模块
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(ProcessExceptionJob).Assembly));
            //属性注入
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            //.PropertiesAutowired();
            builder.RegisterType<QRCodeHub>().ExternallyOwned();
            var container = builder.Build();
            
            AutofacContainer = container;
            //给SignalR 设置依赖处理器
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);

            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            #endregion
        }
    }
}