using Model.DTO.Result;
using Model.Enum;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Models.Authorizes
{

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
  public class TokenAuthorize : AuthorizeAttribute
  {
    public ICacheHelper Cache { get; set; }
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
      string token;
      token = httpContext.Request.Headers["Access"];
      token = token ?? httpContext.Request["Access"];
      token = token ?? httpContext.Request.Cookies["Access"]?.Value;

      if (token == null)
      {
        return false;
      }
      else
      {
        //解析token
        ValidateTokenResult obj = JwtUtils.DecodingToken(token);
        if (obj.ResultCode == ResultCode.Ok)  //验证通过
        {
          //延长token时间
          Cache.SetCache(token, obj.Payload, obj.Payload.Exp);
          return true;
         
        }
        else   //拦截请求
        {
          return false;
        }
      }
    }
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
      base.OnAuthorization(filterContext);
    }
  }
}