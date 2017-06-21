
using Common;
using Common.Cache;
using IBLL;
using Model;
using SmartClass.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Controllers
{
    //[MyActionFilter]
    public class LogonController : Controller
    {

        //public LogonController(ISys_UserService UserService, ISys_UserLogOnService UserLogService)
        //{
        //    this.UserService = UserService;
        //    this.UserLogService = UserLogService;
        //}
        public ISys_UserService UserService { get; set; }
        public ISys_UserLogOnService UserLogService { get; set; }
        //public LogonController(ISys_UserBll UserBll)
        //{
        //    this.UserBll = UserBll;
        //}
        // GET: Logon
        public virtual ActionResult Index()
        {
            return View();
        }
        public void Test()
        {
            MemcacheHelper mc = new MemcacheHelper();
            mc.AddCache("username", "123456");
            string str = mc.GetCache("username").ToString();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logon(string Account, string Pwd, string imei)
        {
            //测试初始化登录-begin
            Account = "admin";
            Pwd = "4a7d1ed414474e4033ac29ccb8653d9b";

            Sys_User User = UserService.GetEntityByAccount(Account);
            if (User == null)
            {
                return Json(new { Message = "用户名不存在" });
            }
            Sys_UserLogOn UserLogOn = UserLogService.GetEntityByUserId(User.F_Id);
            if (UserLogOn == null)
            {
                return Json(new { Message = "查询不到密码信息" });
            }
            string key = UserLogOn.F_UserSecretkey;
            string _Pwd = Md5.md5(DESEncrypt.Encrypt(Pwd, key).ToLower(), 32).ToLower();

            if (UserLogOn.F_UserPassword == _Pwd)    //登录成功
            {
                if (UserLogOn.F_LastVisitTime != null)
                {
                    UserLogOn.F_PreviousVisitTime = UserLogOn.F_LastVisitTime;
                }
                UserLogOn.F_LastVisitTime = DateTime.Now;
                UserLogOn.F_LogOnCount = UserLogOn.F_LogOnCount + 1;
                UserLogService.UpdateEntityInfo(UserLogOn);

                string token = JwtUtils.CreateToken(User, UserLogOn, imei);

                Common.Cache.CacheHelper.AddCache(User.F_Account, token);
                return Json(new
                {
                    Message = "登录成功",
                    Token = token
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "密码错误" });
            }
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateCode()
        {
            Common.ValidateCode validate = new Common.ValidateCode();
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