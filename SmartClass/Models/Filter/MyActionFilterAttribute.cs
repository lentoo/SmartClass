using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Models
{
    public class MyActionFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 行为执行后
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            string method = filterContext.HttpContext.Request.HttpMethod;
        }
        /// <summary>
        /// 行为执行前
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = filterContext.HttpContext.Request["t"];
            //解密
            string[] ss = token.Split('.');

            if (JwtUtils.DecryToken(token))
            {

            }
            else
            {
                var json = new JsonResult();
                json.Data = new { Message = "请重新登录" };
                filterContext.Result = json;
            }
        }
    }
}