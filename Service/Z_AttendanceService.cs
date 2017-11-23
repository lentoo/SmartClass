using SmartClass.Infrastructure.Exception;
using SmartClass.IService;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartClass.Infrastructure.Cache;
using Model.DTO.Courses;
using Model.DTO.Result;
using Model.Enum;
using Model.AutoMapperConfig;
using Model.DTO.Attend;

namespace SmartClass.Service
{
  /// <summary>
  /// 考勤服务对象
  /// </summary>
  public partial class Z_AttendanceService
  {
    public IZ_CourseService CourseService { get; set; }
    public IZ_StudentService StudentService { get; set; }
    public IZ_ClassService ClassService { get; set; }
    public IZ_AttendanceDetailsService AttendanceDetailsService { get; set; }
    public ISys_UserService UserService { get; set; }
    public IZ_RoomService RoomService { get; set; }
    /// <summary>
    /// 发起签到
    /// </summary>
    /// <param name="TeacherNum">教工编号</param>
    /// <returns></returns>
    public AttendanceResult InitiatedAttendance(string TeacherNum, string CourseNo)
    {
      var user = UserService.GetEntity(u => u.F_Account == TeacherNum || u.F_RealName == TeacherNum).FirstOrDefault();
      if (user == null)
      {
        return null;
      }
      Z_Attendance attendance = new Z_Attendance();
      AttendanceResult Result = new AttendanceResult();
      try
      {
        //获取今天与开学的时间状态信息
        SchollTime schollTime = CourseService.GetSchollTime();
        //获取该老师的课程
        List<Course> courses = CourseService.GetTeacherCourse(user.F_Account);

        if (courses == null) //今天没课，不能发起签到
        {
          Result.ResultCode = ResultCode.Error;
          Result.Message = "今日没课，发起签到失败";
          return Result;
        }
        courses = CourseService.SelectCourseInTheCurrentWeek(courses, schollTime);
        //获取该老师今天的课程
        Course course = courses.Where(u => u.Week == schollTime.Week).FirstOrDefault(u => u.EnCode == CourseNo);

        if (course == null)
        {
          Result.ResultCode = ResultCode.Error;
          Result.Message = "今日没课，发起签到失败";
          return Result;
        }
        else //今天有课
        {
          // 通过今天日期和课程ID来确定考勤ID
          attendance.F_ID = $"{schollTime.CurrentTime:yyyyMMdd}.{course.Id}";
          //判断今天是否已经发起签到了
          var a = GetEntity(u => u.F_ID == attendance.F_ID).FirstOrDefault();
          if (a != null)
          {
            Result.ResultCode = ResultCode.Error;
            Result.Message = "发起签到失败，该节课已经发起签到了";
            return Result;
          }
          //获取该课程的节次时间
          List<Z_SectionTime> list = CourseService.GetSectionTime();
          Z_SectionTime sectionTime = list.FirstOrDefault(t => t.F_CourseTimeType.Contains(course.CourseTimeType));
          DateTime sectionDateTime = DateTime.Parse(sectionTime?.F_Time);
          if (sectionDateTime > schollTime.CurrentTime) //上课前
          {
            //上课前10分钟可以发起签到
            TimeSpan time = sectionDateTime - schollTime.CurrentTime;
            if (time.Minutes <= 10)
            {
              attendance.F_TNum = user.F_Account;
              attendance.F_CourseNo = course.EnCode;
              attendance.F_Flag = true;
              attendance.F_ClassRoomNo = course.RoomNo;
              attendance.F_ClassNo = course.Major + course.Classes;
              attendance.F_InitiatedTime = schollTime.CurrentTime;
              AddEntity(attendance);
              // 添加该课程的学生的签到初始化情况到签到详情表
              #region 添加该课程的学生的签到初始化情况到签到详情表
              // 获取该课程下的所有学生
              var studends = CourseService.GetStudentsByCourseId(course);
              List<Z_AttendanceDetails> attendDetails = new List<Z_AttendanceDetails>();
              foreach (var stu in studends)
              {
                Z_AttendanceDetails attendanceDetails = new Z_AttendanceDetails();
                attendanceDetails.F_ID = Guid.NewGuid().ToString("N");
                attendanceDetails.Z_A_F_ID = attendance.F_ID;
                attendanceDetails.F_StuName = stu?.F_StuName;
                attendanceDetails.F_StuNo = stu.F_StuNo;
                attendanceDetails.F_Flag = true;
                attendanceDetails.F_AttenTime = schollTime.CurrentTime;
                attendanceDetails.F_Result = "未进行签到";
                attendanceDetails.F_ClassNo = stu.Z_Class.F_ClassNo;
                attendDetails.Add(attendanceDetails);
              }
              AttendanceDetailsService.AddEntitys(attendDetails);
              #endregion
              Result.RoomNo = course.RoomNo;
              Result.ResultCode = ResultCode.Ok;
              Result.Message = "发起签到成功";
              Result.AttendanceId = attendance.F_ID;
            }
          }
          else  //上课后
          {
            Result.ResultCode = ResultCode.Error;
            Result.Message = "已经上课，不能发起签到";
          }
        }
      }
      catch (Exception exception)
      {
        Result.ResultCode = ResultCode.Error;
        Result.Message = "发起签到失败";
        Result.Error = exception;
        ExceptionHelper.AddException(exception);
        return Result;
      }
      return Result;
    }

    /// <summary>
    /// 通过条件过滤数据
    /// </summary>
    /// <param name="time">时间</param>
    /// <param name="teacher">教师</param>
    /// <param name="section">节次</param>
    /// <param name="building">楼栋</param>
    /// <param name="floor">楼层</param>
    /// <param name="room">教室</param>
    /// <param name="courses">课程列表</param>
    /// <returns>课程列表</returns>
    private List<Course> FilterForCondition(SchollTime time = null, string teacher = null, string section = null, string building = null, string floor = null, string room = null, List<Course> courses = null)
    {
      courses = courses == null ? new List<Course>() : courses;
      // 有时间筛选条件
      courses = time == null ? CourseService.GetToDayCourseOrByDate() : CourseService.GetToDayCourseOrByDate(time);
      // 有房间筛选条件
      courses = building == null ? courses : FilterByRooms(building, floor, room, courses);
      // 有教师筛选条件
      courses = teacher == null ? courses : courses.Where(course => course.TeacherName == teacher).ToList();
      // 有节次筛选条件      
      courses = section == null ? courses : courses.Where(u => u.CourseTimeType.Contains(section)).ToList();
      return courses;
    }
    /// <summary>
    /// 通过房间过滤
    /// </summary>
    /// <param name="building">楼栋</param>
    /// <param name="floor">楼层</param>
    /// <param name="room">教室</param>
    /// <param name="courses">课程列表</param>
    /// <returns></returns>
    private List<Course> FilterByRooms(string building, string floor, string room, List<Course> courses)
    {
      if (courses == null) throw new ArgumentNullException(nameof(courses));
      var classrooms = RoomService.GetRoomsForBuildingName(building, floor, room);
      courses = courses.Where(course => classrooms.Any(c => c.F_RoomNo == course.RoomNo)).ToList();
      return courses;
    }
    /// <summary>
    /// 获取签到详情
    /// </summary>
    /// <param name="teacher">教师</param>
    /// <param name="time">时间</param>
    /// <param name="section">节次</param>
    /// <returns></returns>
    public AttendanceDetailResult AttendanceRecord(string teacher, string time, string section)
    {
      Sys_User user = null;
      if (teacher != null)
      {
        user = UserService.GetEntity(u => u.F_Account == teacher || u.F_RealName == teacher).FirstOrDefault();
      }
      SchollTime schollTime = CourseService.GetSchollTime(time);
      var courses = FilterForCondition(schollTime, teacher, section);
      var result = new AttendanceDetailResult();
      
      if (courses == null || courses.Count() == 0)
      {
        result.Message = "查询不到符合该条件的课程";
        return result;
      }
      var attendanceDetails = GetAttendanceDetailsForCourses(courses, schollTime, user);
      if (attendanceDetails == null || attendanceDetails.Count() == 0)
      {
        result.Message = "该教师今天有课程，但并没有发起签到";
        result.ResultCode = ResultCode.Error;
      }
      else
      {
        result.Data = attendanceDetails;
        result.ResultCode = ResultCode.Ok;
      }
      return result;
    }
    /// <summary>
    /// 通过课程列表获取签到详情
    /// </summary>
    /// <param name="courses">课程列表</param>
    /// <param name="schollTime"></param>
    /// <param name="_user"></param>
    /// <returns></returns>
    private AttendanceDetails[] GetAttendanceDetailsForCourses(List<Course> courses, SchollTime schollTime, Sys_User _user = null)
    {
      List<AttendanceDetails> attendanceDetails = new List<AttendanceDetails>();
      foreach (var course in courses)
      {
        Sys_User user = null;
        if (_user == null)
        {
          user = UserService.GetEntity(u => u.F_RealName == course.TeacherName).FirstOrDefault();
        }
        else
        {
          user = _user;
        }
        // 通过今天日期和课程ID来确定考勤ID
        string attendanceID = $"{schollTime.CurrentTime:yyyyMMdd}.{course.Id}";
        // 查询该老师今天是否有发起签到记录
        var attends = GetEntity(a => a.F_TNum == user.F_Account && a.F_ID == attendanceID);
        if (attends.Count() == 0)  //该教师今天有课程，但并没有发起签到
        {
          continue;
        }
        else   //该教师今天有课程，并发起了签到
        {
          //获取该发起的签到的签到详情
          var attendDetails = AttendanceDetailsService.GetEntity(u => attends.Any(att => att.F_ID == u.Z_A_F_ID)).ToArray();

          var attendanceDetailsArr = AutoMapperConfig.Map<AttendanceDetails[]>(attendDetails);
          attends.ToList().ForEach(att =>
          {
            foreach (var item in attendanceDetailsArr)
            {
              if (item.AttendanceID == att.F_ID)
              {
                item.ClassName = att.F_ClassNo;
                item.ClassRoomNo = att.F_ClassRoomNo;
                item.CourseNo = att.F_CourseNo;
                item.TeacherNo = user.F_Account;
                item.Teacher = user.F_RealName;
                item.CourseName = course.CourseName;
                item.Section = course.CourseTimeType;
              }
            }
          });
          attendanceDetails.AddRange(attendanceDetailsArr);
        }
      }
      return attendanceDetails?.OrderBy(u => u.Section).ToArray();
    }
    public AttendanceDetailResult AttendanceRecordForRooms(string building, string floor, string room, string time, string teacher, string section)
    {
      AttendanceDetailResult result = new AttendanceDetailResult();

      // 获取该楼栋下的所有房间
      var classrooms = RoomService.GetRoomsForBuildingName(building, floor, room);
      SchollTime schollTime = CourseService.GetSchollTime(time);
      List<Course> courses = null;
      // 筛选出在该房间下的所有课程
      courses = FilterForCondition(time: schollTime, teacher: teacher, section: section, building: building, floor: floor, room: room, courses: courses);

      if (courses == null || courses.Count == 0)
      {
        result.Message = "查询不到符合该条件的课程";
        result.ResultCode = ResultCode.Error;
        return result;
      }
      // 获取该课程的签到详情
      AttendanceDetails[] attendanceDetails = GetAttendanceDetailsForCourses(courses, schollTime);
      if (attendanceDetails == null || attendanceDetails.Count() == 0)
      {
        result.Message = "有课，并未发起签到";
        result.ResultCode = ResultCode.Error;
      }
      else
      {
        result.Data = attendanceDetails;
        result.ResultCode = ResultCode.Ok;
      }
      return result;
    }
    /// <summary>
    /// 学生签到
    /// </summary>
    /// <param name="StuNo">学号</param>
    /// <param name="CourseNo">课程编号</param>
    /// <param name="CheckStatus">签到状态</param>
    /// <returns></returns>
    public AttendanceResult CheckIn(string AttendanceId, string StuNo, string CourseNo, string CheckStatus = null)
    {
      AttendanceResult Result = new AttendanceResult();
      try
      {
        SchollTime schollTime = CourseService.GetSchollTime();
        DateTime currentTime = schollTime.CurrentTime;

        Z_Student student = StudentService.GetEntity(u => u.F_StuNo == StuNo).FirstOrDefault();

        Z_Class cClass = ClassService.GetEntity(u => u.F_Id == student.Z_C_F_Id).FirstOrDefault();

        //学生今日的课程
        var courses = CourseService.GetStudentCourse(StuNo);
        courses = CourseService.SelectCourseInTheCurrentWeek(courses, schollTime);
        var course = courses.FirstOrDefault(u => u.Week == schollTime.Week && u.EnCode == CourseNo);

        if (course == null) //学生今日没有该课
        {
          Result.ResultCode = ResultCode.Error;
          Result.Message = "今日没该课程，签到失败";
        }
        else
        {
          var attendance = GetEntity(u => u.F_ID == AttendanceId).FirstOrDefault();
          if (attendance == null)
          {
            Result.ResultCode = ResultCode.Error;
            Result.Message = "签到失败，该教师未发起签到";
            return Result;
          }
          if (course.EnCode != attendance.F_CourseNo)
          {
            Result.ResultCode = ResultCode.Error;
            Result.Message = "签到失败，与发起ID不匹配";
            return Result;
          }
          //获取该课程的节次时间
          List<Z_SectionTime> list = CourseService.GetSectionTime();
          Z_SectionTime sectionTime = list.FirstOrDefault(t => t.F_CourseTimeType.Contains(course.CourseTimeType));

          DateTime sectionDateTime = DateTime.Parse(sectionTime?.F_Time);
          //判断该学生是否已经签到过
          var attend = AttendanceDetailsService
              .GetEntity(a => a.Z_A_F_ID == AttendanceId).FirstOrDefault(u => u.F_StuNo == StuNo);
          if (attend != null && "未进行签到" != attend.F_Result) //已经签到过，不能再进行签到
          {
            if (!string.IsNullOrEmpty(CheckStatus)) //教师给学生进行改变签到状态
            {
              attend.F_Result = CheckStatus;
              AttendanceDetailsService.UpdateEntityInfo(attend);
              Result.ResultCode = ResultCode.Ok;
              Result.Message = "修改签到信息成功";
              return Result;
            }
            Result.ResultCode = ResultCode.Error;
            Result.Message = "签到失败，该节课已经签到";
            return Result;
          }

          //上课前10分钟可以发起签到
          TimeSpan time = sectionDateTime - currentTime;
          //attendanceDetails.F_ID = Guid.NewGuid().ToString();
          //attendanceDetails.Z_A_F_ID = AttendanceId;
          //attendanceDetails.F_StuName = student?.F_StuName;
          //attendanceDetails.F_StuNo = StuNo;
          attend.F_AttenTime = currentTime;
          //attendanceDetails.F_ClassNo = cClass?.F_ClassNo;
          Result.Message = "签到成功";
          Result.RoomNo = course.RoomNo;
          Result.ResultCode = ResultCode.Ok;
          if (time.Minutes >= 0) //上课前
          {
            attend.F_Result = "正常签到";
          }
          else if (time.Minutes < 0) //迟到 
          {
            attend.F_Result = time.Minutes < -120 ? "旷课" : "迟到";
          }
          AttendanceDetailsService.UpdateEntityInfo(attend);
        }

      }
      catch (Exception ex)
      {
        Result.ResultCode = ResultCode.Error;
        Result.Error = ex;
      }
      return Result;
    }

    /// <summary>
    /// 手动签到
    /// </summary>
    /// <param name="TeaNo">教师编号</param>
    /// <param name="StuNo">学生编号</param>
    /// <param name="CourseNo">课程编号</param>
    public AttendanceResult ManualCheckIn(string TeaNo, string StuNo, string CourseNo, string CheckStatus)
    {
      AttendanceResult result = new AttendanceResult();
      DateTime currentTime = Convert.ToDateTime(Infrastructure.Extended.DatetimeExtened.GetNetDateTime());
      //今天星期几
      string week = ((float)currentTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
      Course teacherCourse = CourseService.GetTeacherCourse(TeaNo).FirstOrDefault(u => u.Week == week);
      if (teacherCourse == null) //教师今日没有该课程
      {
        result.ResultCode = ResultCode.Error;
        result.Message = "教师今日没有该课程";
        return result;
      }
      string attendanceId = $"{currentTime:yyyyMMdd}.{teacherCourse.Id}";
      result = CheckIn(attendanceId, StuNo, CourseNo, CheckStatus);
      return result;
    }
  }
}
