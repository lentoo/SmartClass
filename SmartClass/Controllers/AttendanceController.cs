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
using SmartClass.Infrastructure.Mac;

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
            string HostIP = IPUtils.GetHostAddresse();
            string data = $"http://{HostIP}:8080/api/Attendance/StudentCheckIn?AttendanceId={result.AttendanceId}&CourseNo={CourseNo}&StuNo=";
            byte[] bytes = QRCodeHelper.GetQRCode(data);

            var room = RoomService.GetEntity(u => u.F_EnCode == result.RoomNo).FirstOrDefault();
            string mac = room?.F_ComputeMac;
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
          result.Message = "教室网页已断开连接";
          result.AttendanceId = null;
          tran.Rollback();
        }
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
    /// 教师给学生进行签到
    /// </summary>
    /// <param name="TeaNo">教师编号</param>
    /// <param name="StuNo">学生编号</param>
    /// <param name="CourseNo">课程编号</param>
    /// <param name="CheckStatus">状态</param>
    /// <returns></returns>
    public ActionResult ManualCheckIn(string TeaNo, string StuNo, string CourseNo, string CheckStatus)
    {
      var result = AttendanceService.ManualCheckIn(TeaNo, StuNo, CourseNo, CheckStatus);
      return Json(result, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// 查询签到详情信息
    /// </summary>
    /// <param name="teacher">教工名称</param>
    /// <param name="time">时间</param>
    /// <param name="section">节次</param>
    /// <returns></returns>
    public ActionResult AttendanceRecord(string teacher, string time, string section)
    {
      var attends = AttendanceService.AttendanceRecord(teacher, time, section);

      return Json(attends, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AttendanceRecordByRooms(string building, string floor, string room, string time, string teacher, string section)
    {
      var result = AttendanceService.AttendanceRecordForRooms(building, floor, room, time, teacher, section);
      return Json(result, JsonRequestBehavior.AllowGet);
    }
  }
}