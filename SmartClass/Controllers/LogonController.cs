using Common;
using Common.Cache;
using IBLL;
using Model;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logon(string account, string Pwd, string imei)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (Pwd == null) throw new ArgumentNullException(nameof(Pwd));
            //TODO 最终上线要删除
            //测试初始化登录-begin
            account = "admin";
            Pwd = "4a7d1ed414474e4033ac29ccb8653d9b";

            Sys_User user = UserService.GetEntity(u=>u.F_Account==account).FirstOrDefault();
            if (user == null)
            {

                return Json(new LoginResult { Message = "用户名不存在", Status = false });
            }
            Sys_UserLogOn userLogOn = UserLogService.GetEntityByUserId(user.F_Id);

            if (userLogOn == null)
            {
                return Json(new LoginResult() { Message = "查询不到密码信息", Status = false });
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
                    Status = false,
                    AppendData = token
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new LoginResult() {Message = "用户名密码错误", Status = false});
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