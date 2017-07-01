
using Autofac;
using Autofac.Integration.Mvc;
using Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

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
            filterContext.Result = new RedirectResult("/Home/Index");
            //filterContext.HttpContext.Response.Redirect();
        }

        /// <summary>
        /// 开启一个线程 扫描异常队列中的异常信息
        /// </summary>
        public static void ProcessException()
        {
            //采用 Log4Net 日志
            //  ILog log = LogManager.GetLogger("WebLogger");
            //采用 NLog 日志     
            ThreadPool.QueueUserWorkItem((o) =>
            {
                while (true)
                {
                    if (ExceptionHelper.ExceptionQueue.Count > 0)
                    {
                        Exception ex = ExceptionHelper.ExceptionQueue.Dequeue();
                        if (ex != null)
                        {
                            LogHelper.Debug(ex);
                            Thread.Sleep(1000);
                            //TODO: 采用NLog的话，自带了邮件通知系统  发送邮件通知
                            // Common.Email.SendEmail("ERROR", ex.Message);
                        }
                        else
                        {
                            Thread.Sleep(3000);
                        }
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }
                }
            });
        }
    }
}