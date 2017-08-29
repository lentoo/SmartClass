
using Autofac;
using Autofac.Integration.Mvc;
using SmartClass.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using SmartClass.Infrastructure.Exception;
using SmartClass.Infrastructure.Logged;

namespace SmartClass.Models.Exceptions
{
    /// <summary>
    /// 全局异常处理类
    /// </summary>
    public class MyException : HandleErrorAttribute
    {

       // public static Queue<Exception> ExceptionQueue = new Queue<Exception>();

        public static ILogHelper LogHelper = DependencyResolver.Current.GetService<ILogHelper>();      

        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            //获取异常对象
            Exception exception = filterContext.Exception;
            //将异常信息放到异常队列中
            ExceptionHelper.ExceptionQueue.Enqueue(exception);
            //TODO: 此处应该返回500错误页
            //filterContext.Result = new RedirectResult("/Home/Index");
            //filterContext.HttpContext.Response.Redirect();
        }
    }
}