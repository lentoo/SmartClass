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
            string requestUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;
        }
    }
}