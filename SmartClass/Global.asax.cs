using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SmartClass.Models.AutofacConfig;
using SmartClass.Models.Job;
using Model;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Web.Http;
using SmartClass.Models;

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
            //扫描异常信息 改用Quartz定时扫描
            // MyException.ProcessException();

            //执行计划任务
            QuartzConfig.InitJob();
            QuartzConfig.StartJob();

            //EF Pre-Generated Mapping Views（预生成映射视图）
            using (var dbcontext = new NFineBaseEntities())
            {
                var objectContext = ((IObjectContextAdapter)dbcontext).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            //停止所有任务
            QuartzConfig.StopJob();
            SerialPortUtils.ClosePort();
        }
    }
}
