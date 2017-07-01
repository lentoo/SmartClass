using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Models
{
    public class LoginActionFilterAttribute : ActionFilterAttribute
    {
        public ISys_UserService Sys_UserService { get; set; }
        public ISys_LogService Sys_LogService { get; set; }
        Payload payload { get; set; }
        Sys_Log log { get; set; }
        string action { get; set; }
        /// <summary>
        /// 行为执行后，记录操作记录
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            action = filterContext.RouteData.Values["action"].ToString();
            JsonResult result = filterContext.Result as JsonResult;
            ModelResult mr = null;
            string UserHostAddress = filterContext.HttpContext.Request.UserHostAddress;//IP地址
            if (result != null)
            {
                if ("Logon" == action)
                {
                    mr = result.Data as LoginResult;
                    log.F_Account = "admin";
                }
                //else
                //{
                //    mr = result.Data as OperationActuator;
                //    if (payload != null)
                //    {
                //        log.F_Account = payload.Account;
                //    }
                //}
                #region 开启一个线程，后台处理日志信息
                ThreadPool.QueueUserWorkItem((o =>
                {
                    try
                    {
                        log.F_Id = Guid.NewGuid().ToString();
                        log.F_Date = DateTime.Now;
                        log.F_IPAddress = UserHostAddress;
                        log.F_Type = action.ToString();
                        log.F_ModuleName = "系统登录";
                        log.F_IPAddressName = "广东省河源市 电信";
                        log.F_Result = mr.Status;
                        log.F_Description = mr.Message;
                        Sys_User user = Sys_UserService.GetEntityByAccount(log.F_Account);
                        log.F_NickName = user.F_NickName;
                        //更新日志信息
                        Sys_LogService.AddEntity(log);

                    }
                    catch (Exception ex)
                    {
                        ExceptionHelper.AddException(ex);
                    }
                }));
                #endregion
            }
        }

        /// <summary>
        /// 行为执行前
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            action = filterContext.RequestContext.RouteData.Values["action"].ToString();
            log = new Sys_Log();
            log.F_CreatorTime = DateTime.Now;
            if ("Logon" == action)//访问的是登录页面，不验证请求头
            {
                ;
            }
            //else
            //{
            //    string token = filterContext.HttpContext.Request["Access"];
            //    //从缓存中获取token信息
            //    object secret = CacheHelper.GetCache(token);
            //    if (secret != null)
            //    {
            //        //解析token
            //        object obj = JwtUtils.DecodingToken(token, secret.ToString());
            //        if (obj is Payload)  //验证通过
            //        {
            //            payload = obj as Payload;

            //        }
            //        else   //拦截请求
            //        {
            //            var json = new JsonResult();
            //            json.Data = obj;
            //            filterContext.Result = json;
            //        }
            //    }
            //    else    //拦截请求
            //    {
            //        var json = new JsonResult();
            //        json.Data = new { Message = "请重新登录" };
            //        filterContext.Result = json;
            //    }
            
        }
    }
}