using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using Model;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.Enum;
using SmartClass.Models.Filter;
using SmartClass.Infrastructure.Extended;
using Model.DTO.Result;

namespace SmartClass.Controllers
{
    /// <summary>
    /// 登录控制器
    /// </summary>
    [LoginActionFilter]
    public class LogonController : Controller
    {
        public ISys_UserService UserService { get; set; }
        public ICacheHelper Cache { get; set; }
        public ISys_UserLogOnService UserLogService { get; set; }

        /// <summary>
        /// 校验用户是否登录过
        /// </summary>
        /// <param name="access">token</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckToken(string access)
        {
            string token;
            token = HttpContext.Request.Headers["Access"];
            token = token ?? HttpContext.Request["Access"];
            token = token ?? HttpContext.Request.Cookies["Access"]?.Value;

            LoginResult loginResult;
            if (token == null)
            {
                loginResult = new LoginResult()
                {
                    ResultCode = ResultCode.Error,
                    Message = "请重新登录",
                    Status = false
                };
                return Json(loginResult);
            }
            //从缓存中通过token获取用户信息
            Sys_UserLogOn userLogOn = Cache.GetCache<Sys_UserLogOn>(token);
            if (userLogOn != null)
            {
                //解析token
                object obj = JwtUtils.DecodingToken(token, userLogOn.F_UserSecretkey);
                if (obj is Payload)  //验证通过
                {
                    //payload = obj as Payload;
                    Cache.SetCache(token, userLogOn, DateTime.Now.AddDays(7));
                    loginResult = new LoginResult()
                    {
                        ResultCode = ResultCode.Ok,
                        Status = false,
                        Message = "登录成功",
                    };
                    return Json(loginResult);
                }
                loginResult = new LoginResult()
                {
                    ResultCode = ResultCode.Error,
                    Message = "请重新登录",
                    ErrorData = obj
                };
                return Json(loginResult);
            }
            loginResult = new LoginResult()
            {
                ResultCode = ResultCode.Error,
                Status = false,
                Message = "请重新登录",
            };
            return Json(loginResult);
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Logon(string account, string Pwd, string imei)
        {
            //TODO 最终上线要删除
            //测试初始化登录-begin
            //account = "admin";
            //Pwd = "4a7d1ed414474e4033ac29ccb8653d9b";
            Sys_User user = UserService.GetEntity(u => u.F_Account == account).FirstOrDefault();
            LoginResult loginResult;
            if (user == null)
            {
                loginResult = new LoginResult()
                {
                    Message = "用户不存在",
                    Status = false,
                    ResultCode = ResultCode.Error
                };
                return Json(loginResult);
            }
            Sys_UserLogOn userLogOn = UserLogService.GetEntityByUserId(user.F_Id);

            if (userLogOn == null)
            {
                loginResult = new LoginResult()
                {
                    Message = "查询不到密码信息",
                    Status = false,
                    ResultCode = ResultCode.Error
                };
                return Json(loginResult);
            }
            string key = userLogOn.F_UserSecretkey;
            string pwd = DESEncrypt.Encrypt(Pwd, key).ToLower().ToMd5().ToLower();

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
                Cache.AddCache(token, userLogOn, DateTime.Now.AddDays(7));
                HttpCookie tokenCookie = new HttpCookie("Access");
                tokenCookie.Value = token;
                tokenCookie.Domain= "/";
                tokenCookie.Path = "/";
                tokenCookie.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Add(tokenCookie);
                loginResult = new LoginResult
                {
                    Message = "登录成功",
                    Status = true,
                    AppendData = token,
                    ResultCode = ResultCode.Ok
                };
                return Json(loginResult);
            }
            loginResult = new LoginResult() { Message = "用户名密码错误", Status = false, ResultCode = ResultCode.Error };
            return Json(loginResult);

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