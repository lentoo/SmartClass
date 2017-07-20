using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Common;
using Common.Exception;
using IBLL;
using Model;
using Model.Result;

namespace SmartClass.Models.Filter
{
    public class LoginActionFilterAttribute : ActionFilterAttribute
    {
        public ISys_UserService SysUserService { get; set; }
        public ISys_LogService SysLogService { get; set; }
        Sys_Log Log { get; set; }
        string Action { get; set; }
        /// <summary>
        /// 行为执行后，记录登录记录
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            Action = filterContext.RouteData.Values["action"].ToString();
            JsonResult result = filterContext.Result as JsonResult;
            //TODO 这边需要改成从请求头和Cookie获取
            //string account = filterContext.HttpContext.Request["Account"] == null ? filterContext.HttpContext.Request.Cookies["Account"]?.Value : filterContext.HttpContext.Request["Account"];
            ModelResult mr;
            string userHostAddress = filterContext.HttpContext.Request.UserHostAddress;//IP地址
            if (result != null)
            {
                mr = result.Data as LoginResult;
                Log = new Sys_Log { F_Account = "admin" };
                #region 开启一个线程，后台处理日志信息
                ThreadPool.QueueUserWorkItem((o =>
                {
                    try
                {
                    Log.F_Id = Guid.NewGuid().ToString();
                    Log.F_Date = DateTime.Now;
                    Log.F_IPAddress = userHostAddress;
                    Log.F_Type = Action;
                    Log.F_ModuleName = "系统登录";
                    Log.F_IPAddressName = "广东省河源市 电信";
                    Log.F_Result = mr?.Status;
                    Log.F_Description = mr?.Message;
                    Sys_User user = SysUserService.GetEntity(u => u.F_Account == Log.F_Account).FirstOrDefault();
                    Log.F_NickName = user?.F_NickName;
                    //更新日志信息
                    SysLogService.AddEntity(Log);

                }
                catch (Exception ex)
                {
                    ExceptionHelper.AddException(ex);
                }
                }));
                #endregion
            }
        }
    }
}