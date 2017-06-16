using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Controllers
{
    public class LogonController : Controller
    {
        // GET: Logon
        public ActionResult Index()
        {
            return View();
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
                return Json(new {state="No",content="验证码错误" });
            }
            else
            {
                return Json(new { state = "Yes" ,content="验证通过"});
            }
        }
    }
}