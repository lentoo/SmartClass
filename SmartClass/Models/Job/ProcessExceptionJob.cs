using Autofac.Integration.Mvc;
using Common.Exception;
using Common.Logged;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Models.Job
{
    /// <summary>
    /// 处理异常信息工作
    /// </summary>
    public class ProcessExceptionJob : IJob
    {
        /// <summary>
        /// NLog日志类 Autofac自动注入
        /// </summary>
        public ILogHelper LogHelper { get; set; }
        public ProcessExceptionJob(ILogHelper LogHelper)
        {
            this.LogHelper = LogHelper;
        }
        public void Execute(IJobExecutionContext context)
        {
            ProcessExceptionInfo();
        }
        /// <summary>
        /// 异常处理方法
        /// </summary>
        public void ProcessExceptionInfo()
        {
            //判断异常队列是否有异常信息
            if (ExceptionHelper.ExceptionQueue.Count > 0)
            {
                //取出异常信息
                Exception ex = ExceptionHelper.ExceptionQueue.Dequeue();
                if (ex != null)
                {
                    LogHelper.Debug(ex);
                    //TODO: 采用NLog的话，自带了邮件通知系统  发送邮件通知
                    // Common.Email.SendEmail("ERROR", ex.Message);
                }
            }
        }
    }
}