using Autofac;
using Autofac.Extras.Quartz;
using Autofac.Integration.Mvc;

namespace SmartClass.Models.AutofacConfig
{
    public class ServiceGetter:IServiceGetter
    {
        public T GetByName<T>(string name)
        {
            return AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(name);
        }
        public T GetService<T>()
        {
            return AutofacDependencyResolver.Current.RequestLifetimeScope.Resolve<T>();
        }
    }
}