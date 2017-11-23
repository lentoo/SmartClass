using System;
using System.Linq;
using System.Web.Mvc;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using Model;
using Model.Enum;
using SmartClass.Models.Types;
using System.Threading;
using Model.DTO.Result;

namespace SmartClass.Models.Filter
{
  /// <summary>
  /// 操作设备过滤器
  /// </summary>
  public class EquipmentLogFilterAttribute : ActionFilterAttribute
  {
     public IZ_EquipmentLogService ZEquipmentLogService { get; set; }
    public IZ_RoomService ZRoomService { get; set; }
    public IZ_EquipmentService ZEquipmentService { get; set; }
    public ISys_UserService SysUserService { get; set; }
    public ICacheHelper Cache { get; set; }
    private readonly string EQUOPEN = "EquOpen";
    private readonly string EQUCLOSE = "EQuClose";
    private readonly string EQUSEARCH = "EQuSearch";
    public bool isCheck = true;
    /// <summary>
    /// 操作前
    /// </summary>
    /// <param name="filterContext"></param>
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      base.OnActionExecuting(filterContext);
      ////TODO 这边要改成从请求头中获取
      //string token;
      //token = filterContext.HttpContext.Request.Headers["Access"];
      //token = token ?? filterContext.HttpContext.Request["Access"];
      //token = token ?? filterContext.HttpContext.Request.Cookies["Access"]?.Value;
      //if (token == null)
      //{
      //  var json = new JsonResult();
      //  json.Data = new { ResultCode = ResultCode.Error, Message = "请重新登录" };
      //  filterContext.Result = json;

      //}
      //else
      //{
      //  //从缓存中获取token信息
      //  //string UserSecretkey = Cache.GetCache<string>(token);
      //  //if (UserSecretkey != null)
      //  //{
      //    //解析token
      //    ValidateTokenResult obj = JwtUtils.DecodingToken(token);
      //    this.ValidateTokenResult = obj;
      //    if (obj.ResultCode == ResultCode.Ok)  //验证通过
      //    {
      //      //延长token时间
      //      //Cache.SetCache(token, UserSecretkey, DateTime.Now.AddDays(7));
      //    }
      //  else   //拦截请求
      //  {

      //    var json = new JsonResult { Data = obj };
      //    filterContext.Result = json;
      //  }
      ////}
      ////  else    //缓存中没有token信息，则拦截请求
      ////  {
      ////    var json = new JsonResult();
      ////    json.Data = new { ResultCode = ResultCode.Error, Message = "请重新登录" };
      ////    filterContext.Result = json;
      ////  }
      //}
    }
    /// <summary>
    /// 操作后
    /// </summary>
    /// <param name="filterContext"></param>
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
      base.OnActionExecuted(filterContext);
      if (!isCheck) return;
      string token;
      token = filterContext.HttpContext.Request.Headers["Access"];
      token = token ?? filterContext.HttpContext.Request["Access"];
      token = token ?? filterContext.HttpContext.Request.Cookies["Access"]?.Value;
      var payload = Cache.GetCache<Payload>(token);
      //操作日志记录
      JsonResult jsonResult = filterContext.Result as JsonResult;
      string roomId = filterContext.HttpContext.Request["classroom"];
      string nodeId = filterContext.HttpContext.Request["nodeAdd"] ?? "00";
      if (nodeId?.IndexOf("_") != -1)
      {
        nodeId = nodeId.Split('_')[0];
      }
      string onoff = filterContext.HttpContext.Request["onoff"];
      onoff = string.IsNullOrEmpty(onoff) ? "" : onoff;
      EquipmentResult equipmentResult = jsonResult?.Data as EquipmentResult;
      if (equipmentResult != null)
      {
        //开启线程处理后续日志操作
        ThreadPool.QueueUserWorkItem(oo =>
        {
          Z_EquipmentLog zEquipmentLog = new Z_EquipmentLog();
          zEquipmentLog.F_Id = Guid.NewGuid().ToString();
          zEquipmentLog.F_Account = payload.Account;
          zEquipmentLog.F_Date = DateTime.Now;
          zEquipmentLog.F_RoomNo = roomId;
          string roomName = ZRoomService.GetEntity(z => z.F_RoomNo.ToLower() == roomId.ToLower()).Select(z => z.F_FullName).FirstOrDefault();
          string nodeName = ZEquipmentService.GetEntity(e => e.F_EquipmentNo.ToLower() == nodeId.ToLower())
                      .Select(e => e.F_FullName).FirstOrDefault();
          var user = SysUserService.GetEntity(u => u.F_Account == payload.Account).Select(o => new { o.F_NickName, o.F_RealName })
                    .FirstOrDefault();
          zEquipmentLog.F_EquipmentNo = nodeId;
          zEquipmentLog.F_Description = equipmentResult.Message;
          zEquipmentLog.F_EquipmentLogType = onoff == StateType.OPEN ? EQUOPEN : onoff == StateType.CLOSE ? EQUCLOSE : EQUSEARCH;
          zEquipmentLog.F_RoomName = roomName;
          zEquipmentLog.F_EquipmentName = nodeName;
          zEquipmentLog.F_NickName = user?.F_NickName;
          zEquipmentLog.F_FullName = user?.F_RealName;

          ZEquipmentLogService.AddEntity(zEquipmentLog);
        });
      }
    }
  }
}