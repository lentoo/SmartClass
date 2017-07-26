using Autofac;
using Autofac.Extras.Quartz;
using Autofac.Integration.Mvc;

namespace SmartClass.Models.Autofac
{
    public class ServiceGetter:IServiceGetter
    {
        public T GetByName<T>(string name)
        {
            return AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(name);
        }
    }
}