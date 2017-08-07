using Common.Cache;
using Common.Extended;
using IBLL;
using Microsoft.AspNet.SignalR;
using Model;
using Model.Result;
using SmartClass.Models.SignalR;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Model.Enum;
using ThoughtWorks.QRCode.Codec;

namespace SmartClass.Controllers
{
    /// <summary>
    /// 考勤  控制器
    /// </summary>
    public class AttendanceController : Controller
    {
        public IZ_AttendanceService AttendanceService { get; set; }
        public IZ_RoomService RoomService { get; set; }
        public ICacheHelper Cache { get; set; }

        public IZ_ClassComputeService ClassComputeService { get; set; }
        // GET: Attendance
        public ActionResult Index()
        {
            string currentTime = Convert.ToDateTime(DatetimeExtened.GetNetDateTime()).ToString("yyyy/MM/dd hh:mm:ss");
            return Content(currentTime);
        }

        /// <summary>
        /// 教师发起签到
        /// </summary>
        /// <param name="TeaNo">教工编号</param>
        /// <param name="CourseNo">课程编号</param>
        /// <returns></returns>
        public ActionResult InitiatedCheckIn(string TeaNo, string CourseNo)
        {
            if (TeaNo == null || CourseNo == null)
            {
                return null;
            }
            AttendanceResult result = AttendanceService.InitiatedAttendance(TeaNo, CourseNo);
            if (result.ResultCode == ResultCode.Ok)
            {
                QRCodeEncoder encoder = new QRCodeEncoder();
                encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE; //编码方式 (注意：BYTE能支持中文，ALPHA_NUMERIC扫描出来的都是数字)
                encoder.QRCodeScale = 14; //大小(值越大生成的二维码图片像素越高)
                encoder.QRCodeVersion = 0; //版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
                encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M; //错误效验、错误更正(有4个等级)
                //二维码数据
                string qrdata = $"{result.AttendanceId}-{CourseNo}";
                System.Drawing.Bitmap bp = encoder.Encode(qrdata, Encoding.UTF8);
                MemoryStream ms = new MemoryStream();
                bp.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.GetBuffer();

                string roomId = RoomService.GetEntity(u => u.F_EnCode == result.RoomNo).FirstOrDefault()?.F_Id;

                string mac = ClassComputeService.GetEntity(u => u.F_RoomId == roomId).FirstOrDefault()?.F_ComputeMac;
                string connectionId = Cache.GetCache<string>(mac);
                GlobalHost.ConnectionManager.GetHubContext<QRCodeHub>().Clients.Client(connectionId).ReciverImg(bytes);
            }
            //return File(bytes, "image/png");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 学生进行签到
        /// </summary>
        /// <param name="AttendanceId">签到ID</param>
        /// <param name="StuNo">学生学号</param>
        /// <param name="CourseNo">课程编号</param>
        public ActionResult StudentCheckIn(string AttendanceId, string StuNo, string CourseNo)
        {
            AttendanceResult result = AttendanceService.CheckIn(AttendanceId, StuNo, CourseNo);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}