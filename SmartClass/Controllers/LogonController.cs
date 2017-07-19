using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Enum;
using Model.Result;
using SmartClass.Models.Filter;

namespace SmartClass.Controllers
{
    /// <summary>
    /// 登录控制器
    /// </summary>
    [LoginActionFilter]
    public class LogonController : Controller
    {
        public ISys_UserService UserService { get; set; }
        public ISys_UserLogOnService UserLogService { get; set; }

        public ActionResult CheckToken(string access)
        {
            string token;
            token = HttpContext.Request.Headers["Access"];
            token = token ?? HttpContext.Request["Access"];
            token = token ?? HttpContext.Request.Cookies["Access"]?.Value;
            if (token == null)
            {
                return Json(new { ResultCode = ResultCode.Error, Message = "请重新登录" }, JsonRequestBehavior.AllowGet);
            }
            //从缓存中通过token获取用户信息
            Sys_UserLogOn userLogOn = CacheHelper.GetCache<Sys_UserLogOn>(token);
            if (userLogOn != null)
            {
                //解析token
                object obj = JwtUtils.DecodingToken(token, userLogOn.F_UserSecretkey);
                if (obj is Payload)  //验证通过
                {
                    //payload = obj as Payload;
                    CacheHelper.SetCache(token, userLogOn, DateTime.Now.AddDays(7));
                    return Json(new { ResultCode = ResultCode.Ok, Message = "登录成功" });
                }
                return Json(new { ResultCode = ResultCode.Error, Message = "请重新登录", ErrorData = obj }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { ResultCode = ResultCode.Error, Message = "请重新登录" }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logon(string account, string Pwd, string imei)
        {
            //TODO 最终上线要删除
            //测试初始化登录-begin
            account = "admin";
            Pwd = "4a7d1ed414474e4033ac29ccb8653d9b";

            Sys_User user = UserService.GetEntity(u => u.F_Account == account).FirstOrDefault();
            if (user == null)
            {

                return Json(new LoginResult { Message = "用户名不存在", Status = false, ResultCode = ResultCode.Error });
            }
            Sys_UserLogOn userLogOn = UserLogService.GetEntityByUserId(user.F_Id);

            if (userLogOn == null)
            {
                return Json(new LoginResult() { Message = "查询不到密码信息", Status = false, ResultCode = ResultCode.Error });
            }
            string key = userLogOn.F_UserSecretkey;
            string pwd = Md5.md5(DESEncrypt.Encrypt(Pwd, key).ToLower(), 32).ToLower();

            if (userLogOn.F_UserPassword == pwd) //登录成功
            {
                if (userLogOn.F_LastVisitTime != null)
                {
                    userLogOn.F_PreviousVisitTime = userLogOn.F_LastVisitTime;
                }
                userLogOn.F_LastVisitTime = DateTime.Now;
                userLogOn.F_LogOnCount = userLogOn.F_LogOnCount + 1;
                UserLogService.UpdateEntityInfo(userLogOn);
                Payload payload = new Payload()
                {
                    Account = account,
                    IMEI = imei
                };
                //创建一个token
                string token = JwtUtils.EncodingToken(payload, userLogOn.F_UserSecretkey);
                CacheHelper.AddCache(token, userLogOn, DateTime.Now.AddDays(7));
                HttpCookie tokenCookie = new HttpCookie("Access");
                tokenCookie.Value = token;
                tokenCookie.Path = "/";

                tokenCookie.Expires = DateTime.Now.AddDays(7);
                Response.AppendCookie(tokenCookie);
                return Json(new LoginResult
                {
                    Message = "登录成功",
                    Status = true,
                    AppendData = token,
                    ResultCode = ResultCode.Ok
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new LoginResult() { Message = "用户名密码错误", Status = false, ResultCode = ResultCode.Error });
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateCode()
        {
            ValidateCode validate = new ValidateCode();
            string code = validate.CreateValidateCode(4);
            byte[] data = validate.CreateValidateGraphic(code);
            Session["validateCode"] = code;
            return File(data, "image/jpeg");
        }
        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="validateCode"></param>
        /// <returns></returns>
        public ActionResult CheckValidate(string validateCode)
        {
            if (Session["validateCode"] == null)
            {
                return Json(new { state = "No", content = "验证码过期" });
            }
            string code = Session["validateCode"].ToString();
            if (code != validateCode)
            {
                return Json(new { state = "No", content = "验证码错误" });
            }
            else
            {
                return Json(new { state = "Yes", content = "验证通过" });
            }
        }
    }
}