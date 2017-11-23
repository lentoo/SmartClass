using Model;
using Model.DTO.Result;
using System;

namespace SmartClass.IService
{
  public partial interface IZ_AttendanceService : IBaseService<Z_Attendance>
  {
    /// <summary>
    /// 发起签到
    /// </summary>
    /// <param name="TeacherNum">教师编号</param>
    /// <param name="CourseNo">课程编号</param>
    AttendanceResult InitiatedAttendance(string TeacherNum, string CourseNo);

    /// <summary>
    /// 学生签到
    /// </summary>
    /// <param name="StuNo">学号</param>
    /// <param name="CourseNo">课程编号</param>
    /// <returns></returns>
    AttendanceResult CheckIn(string AttendanceId, string StuNo, string CourseNo, string CheckStatus = null);

    /// <summary>
    /// 手动签到
    /// </summary>
    /// <param name="TeaNo">教师编号</param>
    /// <param name="StuNo">学生编号</param>
    /// <param name="CourseNo">课程编号</param>
    /// <param name="ChechStatus">状态</param>
    AttendanceResult ManualCheckIn(string TeaNo, string StuNo, string CourseNo, string CheckStatus);

    AttendanceDetailResult AttendanceRecord(string teacher, string time,string section);
    AttendanceDetailResult AttendanceRecordForRooms(string building, string floor, string room, string time, string teacher, string section);
  }
}
