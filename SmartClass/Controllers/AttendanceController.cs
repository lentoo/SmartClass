using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using Microsoft.AspNet.SignalR;
using SmartClass.Models.SignalR;
using System;
using System.Linq;
using System.Web.Mvc;
using SmartClass.Infrastructure.Exception;
using SmartClass.Infrastructure.Images;
using Model.DTO.Result;
using Model.Enum;

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
            using (var tran = AttendanceService.dal.dbContext.Database.BeginTransaction())
            {
                AttendanceResult result = AttendanceService.InitiatedAttendance(TeaNo, CourseNo);
                try
                {

                    if (result.ResultCode == ResultCode.Ok)
                    {
                        string data = $"{result.AttendanceId}|{CourseNo}";
                        byte[] bytes = QRCodeHelper.GetQRCode(data);

                        string roomId = RoomService.GetEntity(u => u.F_EnCode == result.RoomNo).FirstOrDefault()?.F_Id;

                        string mac = ClassComputeService.GetEntity(u => u.F_RoomId == roomId).FirstOrDefault()
                            ?.F_ComputeMac;
                        string connectionId = Cache.GetCache<string>(mac);

                        GlobalHost.ConnectionManager.GetHubContext<QRCodeHub>().Clients.Client(connectionId)
                            .ReciverImg(bytes);
                        tran.Commit();
                    }
                }
                catch (Exception exception)
                {
                    ExceptionHelper.AddException(exception);
                    result.ResultCode = ResultCode.Error;
                    result.Message = "教室网页没连接";
                    result.AttendanceId = null;
                    tran.Rollback();
                }
                //return File(bytes, "image/png");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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

        /// <summary>
        /// 手动签到
        /// </summary>
        /// <param name="TeaNo">教师编号</param>
        /// <param name="StuNo">学生编号</param>
        /// <param name="CourseNo">课程编号</param>
        /// <returns></returns>
        public ActionResult ManualCheckIn(string TeaNo, string StuNo, string CourseNo)
        {
            var result = AttendanceService.ManualCheckIn(TeaNo, StuNo, CourseNo);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}