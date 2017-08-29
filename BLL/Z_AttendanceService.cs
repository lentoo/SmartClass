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
        /// <summary>
        /// 发起签到
        /// </summary>
        /// <param name="TeacherNum">教工编号</param>
        /// <returns></returns>
        public AttendanceResult InitiatedAttendance(string TeacherNum, string CourseNo)
        {
            Z_Attendance attendance = new Z_Attendance();
            AttendanceResult Result = new AttendanceResult();
            try
            {
                DateTime currentTime = Convert.ToDateTime(SmartClass.Infrastructure.Extended.DatetimeExtened.GetNetDateTime());
                attendance.F_InitiatedTime = currentTime;


                //今天星期几
                string week = ((float)currentTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
                List<Course> courses = CourseService.GetTeacherCourse(TeacherNum);
                Course course = courses.Where(u => u.F_Week == week).FirstOrDefault(u => u.F_EnCode == CourseNo);

                if (course == null) //今天没课，不能发起签到
                {
                    Result.ResultCode = ResultCode.Error;
                    Result.Message = "今日没课，发起签到失败";
                }
                else //今天有课
                {
                    // 通过今天日期和课程ID，课程编号,教师编号来确定考勤ID
                    attendance.F_ID = $"{currentTime.ToString("yyyyMMdd")}|{course.Id}";
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
                    Z_SectionTime sectionTime = list.FirstOrDefault(t => t.F_CourseTimeType == course.F_CourseTimeType);
                    DateTime sectionDateTime = DateTime.Parse(sectionTime?.F_Time);
                    if (sectionDateTime > currentTime) //上课前
                    {
                        //上课前10分钟可以发起签到
                        TimeSpan time = sectionDateTime - currentTime;
                        if (time.Minutes <= 10)
                        {
                            attendance.F_TNum = TeacherNum;
                            attendance.F_CourseNo = course.F_EnCode;
                            attendance.F_Flag = true;
                            attendance.F_ClassRoomNo = course.F_RoomNo;
                            attendance.F_ClassNo = course.Major + course.Classes;
                            attendance.F_InitiatedTime = currentTime;
                            AddEntity(attendance);
                            Result.RoomNo = course.F_RoomNo;
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
        /// 学生签到
        /// </summary>
        /// <param name="StuNo">学号</param>
        /// <param name="CourseNo">课程编号</param>
        /// <returns></returns>
        public AttendanceResult CheckIn(string AttendanceId, string StuNo, string CourseNo)
        {
            AttendanceResult Result = new AttendanceResult();
            try
            {
                DateTime currentTime = Convert.ToDateTime(SmartClass.Infrastructure.Extended.DatetimeExtened.GetNetDateTime());
                Z_AttendanceDetails attendanceDetails = new Z_AttendanceDetails();
                Z_Student student = StudentService.GetEntity(u => u.F_StuNo == StuNo).FirstOrDefault();

                Z_Class cClass = ClassService.GetEntity(u => u.F_Id == student.Z_C_F_Id).FirstOrDefault();
                //今天星期几
                string week = ((float)currentTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
                //学生今日的课程
                Course course = CourseService.GetStudentCourse(StuNo).FirstOrDefault(u => u.F_Week == week && u.F_EnCode == CourseNo);
                if (course == null) //学生今日没有该课
                {
                    Result.ResultCode = ResultCode.Error;
                    Result.Message = "今日没该课程，签到失败";
                }
                else
                {
                    var attendance = GetEntity(u => u.F_ID == AttendanceId).FirstOrDefault();
                    if (course.F_EnCode != attendance?.F_CourseNo)
                    {
                        Result.ResultCode = ResultCode.Error;
                        Result.Message = "签到失败，与发起ID不匹配";
                        return Result;
                    }
                    if (attendance == null)
                    {
                        Result.ResultCode = ResultCode.Error;
                        Result.Message = "签到失败，该教师未发起签到";
                        return Result;
                    }
                    //获取该课程的节次时间
                    List<Z_SectionTime> list = CourseService.GetSectionTime();
                    Z_SectionTime sectionTime = list.FirstOrDefault(t => t.F_CourseTimeType == course.F_CourseTimeType);

                    DateTime sectionDateTime = DateTime.Parse(sectionTime?.F_Time);
                    //判断该学生是否已经签到过
                    var attendanceList = AttendanceDetailsService.GetEntity(a => a.Z_A_F_ID == AttendanceId)
                        .Where(u => u.F_StuNo == StuNo).ToList();
                    if (attendanceList.Count > 0) //已经签到过，不能再进行签到
                    {
                        Result.ResultCode = ResultCode.Error;
                        Result.Message = "签到失败，该节课已经签到";
                        return Result;
                    }
                    //上课前10分钟可以发起签到
                    TimeSpan time = sectionDateTime - currentTime;
                    attendanceDetails.F_ID = Guid.NewGuid().ToString();
                    attendanceDetails.Z_A_F_ID = AttendanceId;
                    attendanceDetails.F_StuName = student?.F_StuName;
                    attendanceDetails.F_StuNo = StuNo;
                    attendanceDetails.F_AttenTime = currentTime;
                    attendanceDetails.F_ClassNo = cClass?.F_ClassNo;
                    Result.Message = "签到成功";
                    Result.RoomNo = course.F_RoomNo;
                    Result.ResultCode = ResultCode.Ok;
                    if (time.Minutes >= 0) //上课前
                    {
                        attendanceDetails.F_Result = "正常签到";
                    }
                    else if (time.Minutes < 0) //迟到 
                    {
                        attendanceDetails.F_Result = time.Minutes < -120 ? "旷课" : "迟到";
                    }
                    attendanceDetails.F_Flag = true;
                    AttendanceDetailsService.AddEntity(attendanceDetails);
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
        public AttendanceResult ManualCheckIn(string TeaNo, string StuNo, string CourseNo)
        {
            AttendanceResult result = new AttendanceResult();
            DateTime currentTime = Convert.ToDateTime(SmartClass.Infrastructure.Extended.DatetimeExtened.GetNetDateTime());
            //今天星期几
            string week = ((float)currentTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
            Course teacherCourse = CourseService.GetTeacherCourse(TeaNo).FirstOrDefault(u => u.F_Week == week);
            if (teacherCourse == null) //教师今日没有该课程
            {
                result.ResultCode = ResultCode.Error;
                result.Message = "教师今日没有该课程";
                return result;
            }
            string attendanceId = $"{currentTime.ToString("yyyyMMdd")}|{teacherCourse.Id}|{CourseNo}|{TeaNo}";
            result = CheckIn(attendanceId, StuNo, CourseNo);
            return result;
        }
    }
}
