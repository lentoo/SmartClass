using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Common;
using Common.Cache;
using IBLL;
using Model;

namespace SmartClass.Models.Filter
{
    /// <summary>
    /// 操作设备过滤器
    /// </summary>
    public class EquipmentLogFilterAttribute : ActionFilterAttribute
    {
        private Payload payload { get; set; }
        public IZ_EquipmentLogService ZEquipmentLogService { get; set; }
        public IZ_RoomService ZRoomService { get; set; }
        public IZ_EquipmentService ZEquipmentService { get; set; }
        public ISys_UserService SysUserService { get; set; }

        /// <summary>
        /// 操作前
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string token = filterContext.HttpContext.Request["Access"];

            //从缓存中获取token信息
            Sys_UserLogOn UserLogOn = CacheHelper.GetCache<Sys_UserLogOn>(token);
            if (UserLogOn != null)
            {
                //解析token
                object obj = JwtUtils.DecodingToken(token, UserLogOn.F_UserSecretkey);
                if (obj is Payload)  //验证通过
                {
                    payload = obj as Payload;

                }
                else   //拦截请求
                {
                    var json = new JsonResult();
                    json.Data = obj;
                    filterContext.Result = json;
                }
            }
            else    //缓存中没有token信息，则拦截请求
            {
                var json = new JsonResult();
                json.Data = new { ResultCode=ResultCode.Error, Message = "请重新登录" };
                filterContext.Result = json;
            }
        }
        /// <summary>
        /// 操作后
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            string action = filterContext.RouteData.Values["action"].ToString();
            JsonResult jsonResult = filterContext.Result as JsonResult;
            string roomId = "0x" + filterContext.HttpContext.Request["classroom"];
            string nodeId = "0x" + filterContext.HttpContext.Request["nodeAdd"];
            EquipmentResult equipmentResult = jsonResult.Data as EquipmentResult;
            if (equipmentResult != null)
            {
                //开启线程处理后续日志操作记录
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Z_EquipmentLog zEquipmentLog = new Z_EquipmentLog();
                    zEquipmentLog.F_Id = Guid.NewGuid().ToString();
                    zEquipmentLog.F_Account = payload.Account;
                    zEquipmentLog.F_Date = DateTime.Now;
                    zEquipmentLog.F_RoomNo = roomId;
                    string roomName = ZRoomService.GetEntity(z => z.F_RoomNo.ToLower() == roomId.ToLower()).Select(z => z.F_FullName).FirstOrDefault();
                    string nodeName = ZEquipmentService.GetEntity(e => e.F_EquipmentType.ToLower() == nodeId.ToLower())
                        .Select(e => e.F_FullName).FirstOrDefault();
                    Sys_User user = SysUserService.GetEntity(u => u.F_Account == payload.Account)
                      .FirstOrDefault();
                    zEquipmentLog.F_EquipmentNo = nodeId;
                    zEquipmentLog.F_Description = equipmentResult.Message;
                    zEquipmentLog.F_LogType = action;
                    zEquipmentLog.F_RoomName = roomName;
                    zEquipmentLog.F_EquipmentName = nodeName;
                    zEquipmentLog.F_NickName = user == null ? "null" : user.F_NickName;
                    zEquipmentLog.F_FullName = user == null ? "null" : user.F_RealName;
                    ZEquipmentLogService.AddEntity(zEquipmentLog);
                });
            }
        }
    }
}