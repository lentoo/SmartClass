using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Extras.Quartz;
using Autofac.Integration.Mvc;
using Common.Cache;
using SmartClass.Models.Job;

namespace SmartClass.Models.Autofac
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

            builder.RegisterType<ServiceGetter>().As<IServiceGetter>();
            builder.RegisterType<SerialPortService>().AsSelf().PropertiesAutowired();
            //注册定时任务模块
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(ProcessExceptionJob).Assembly));
            //属性注入
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            builder.RegisterFilterProvider();//.PropertiesAutowired();
            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));           
            #endregion
        }
    }
}