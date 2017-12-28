using Model;
using Model.Enum;
using SmartClass.Infrastructure;
using SmartClass.Infrastructure.Cache;
using SmartClass.Infrastructure.Extended;
using SmartClass.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SmartClass.Controllers
{

    public class StudentController : Controller
    {
        private readonly IZ_StudentService studentService;
        private readonly ICacheHelper Cache;
        public StudentController(IZ_StudentService studentService, ICacheHelper Cache)
        {
            this.studentService = studentService;
            this.Cache = Cache;
        }
        // GET: Student
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Logon(string StuNo = "0", string Password = "0",string Imei="0000")
        {
            Password = Password.ToMd5();
            var student = studentService.GetEntity(u => u.F_StuNo == StuNo && u.F_StuPassword == Password)
                .Select(o => new { stuNo = o.F_StuNo, stuName = o.F_StuName, phone = o.F_StuPhone, weChat = o.F_WeChat, id = o.F_ID })
                .FirstOrDefault();
            if (student == null)
            {
                return await Task.FromResult(Json(new { ResultCode = ResultCode.Error, Message = "学号或密码错误" }));
            }
            Payload payload = new Payload()
            {
                Account = StuNo,
                Exp = DateTime.Now.AddDays(7),
                IMEI = Imei,
                Issuer = "IServer"
            };
            //创建一个token
            string token = JwtUtils.EncodingToken(payload);
            Cache.AddCache(token, payload, DateTime.Now.AddDays(7));
            return await Task.FromResult(Json(new { ResultCode = ResultCode.Ok, Message = "登录成功", Data = student,Token=token }));
        }
    }
}