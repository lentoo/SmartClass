using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartClass.IService;
using Model;
using System.Threading;
using System.Diagnostics;
using SmartClass.Infrastructure;
using SmartClass.Models.Types;
using SmartClass.Infrastructure.Exception;
using Model.DTO.Courses;

namespace SmartClass.Models.Job
{
  /// <summary>
  /// 定时处理每日课程表
  /// </summary>
  public class TimingProcessDailyCoursesJob : IJob
  {
    private readonly IZ_CourseService CourseService;

    private readonly IZ_SectionTimeService SectionTimeService;
    private readonly SerialPortService PortService;

    public TimingProcessDailyCoursesJob(IZ_CourseService CourseService, IZ_SectionTimeService SectionTimeService, SerialPortService PortService)
    {
      this.CourseService = CourseService;
      this.SectionTimeService = SectionTimeService;
      this.PortService = PortService;
    }
    public void Execute(IJobExecutionContext context)
    {
      try
      {
        List<Course> courses = CourseService.GetToDayCourse();
        Debug.WriteLine("获取成功...");
        Debug.WriteLine($"今日有{courses.Count}节课");
        Debug.WriteLine("正在处理定时任务......");
        ProcessCourseAsync(courses);
      }
      catch (Exception ex)
      {
        ExceptionHelper.AddException(ex);
      }
    }
    /// <summary>
    /// 异步处理今日课程
    /// </summary>
    /// <param name="courses"></param>
    public void ProcessCourseAsync(List<Course> courses)
    {
      List<Course> toDayCourses = courses;
      foreach (Course course in toDayCourses)
      {
        ThreadPool.QueueUserWorkItem(o =>
        {
          DateTime openTime = GetCourseTime(course.CourseTimeType);
          openTime = openTime.AddMinutes(-10);  //提前10分钟打开
          DateTime closeTime = GetClassOver(course, toDayCourses, openTime);
          while (true)
          {
            DateTime currenTime = DateTime.Now;
            if (currenTime.Hour > openTime.Hour)//过了打开时间
            {
              Debug.WriteLine("过了打开时间");
              break;
            }
            if (currenTime.Hour == openTime.Hour && currenTime.Minute > openTime.Minute)
            {
              Debug.WriteLine("过了打开时间");
              break;
            }
            if (currenTime.Hour == openTime.Hour && currenTime.Minute == openTime.Minute) //开启上课命令
            {
              //TODO 发送上课命令
              //Task<string> task = SendCmd(course. RoomNo, "18", "open");
              SetLamp(course.RoomNo, "3", "open");
              Debug.WriteLine($"{course.RoomNo}课室开始上课..");
              break;
            }
            Thread.Sleep(30000);
          }
          while (true)
          {
            DateTime currenTime = DateTime.Now;
            if (currenTime.Hour > closeTime.Hour)
            {
              Debug.WriteLine("过了关闭时间");
              //toDayCourses.Remove(course);
              break;
            }
            if (currenTime.Hour == closeTime.Hour && currenTime.Minute > closeTime.Minute)
            {
              Debug.WriteLine("过了关闭时间");
              //toDayCourses.Remove(course);
              break;
            }
            if (currenTime.Hour == closeTime.Hour && currenTime.Minute == closeTime.Minute) //开启下课命令
            {
              //发送下课命令
              //Task<string> task1 = SendCmd(course.F_RoomNo, "18", "close");
              SetLamp(course.RoomNo, "18", "close");
              Debug.WriteLine($"{course.RoomNo}课室下课..");
              //toDayCourses.Remove(course);
              return;
            }
            Thread.Sleep(30000);
          }
        });
      }
    }
    /// <summary>
    /// 获取最佳关闭时间
    /// </summary>
    /// <param name="course">当前课程</param>
    /// <param name="toDayCourses">今日所有课程</param>
    /// <param name="openTime">打开时间</param>
    /// <returns></returns>
    public DateTime GetClassOver(Course course, List<Course> toDayCourses, DateTime openTime)
    {
      if (course == null) throw new ArgumentNullException("Course为null");
      if (toDayCourses == null) throw new ArgumentNullException("toDayCourses为null");
      if (course.CourseTimeType == CourseTimeType.Section1_2 || course.CourseTimeType == CourseTimeType.Section5_6)
      {
        foreach (var item in toDayCourses)
        {

          if (course.RoomNo == item.RoomNo)
          {
            if (course.CourseTimeType == CourseTimeType.Section1_2 && item.CourseTimeType == CourseTimeType.Section3_4)
            {
              return openTime.AddHours(4);
            }
            if (course.CourseTimeType == CourseTimeType.Section5_6 && item.CourseTimeType == CourseTimeType.Section7_8)
            {
              return openTime.AddHours(4);
            }
          }
        }
        return openTime.AddHours(2);
      }
      if (course.CourseTimeType == CourseTimeType.Section1_4 || course.CourseTimeType == CourseTimeType.Section5_8)
      {
        return openTime.AddHours(4);
      }
      return openTime.AddHours(2);
    }
    /// <summary>
    /// 获取上课时间
    /// </summary>
    /// <param name="courseTimeType"></param>
    /// <returns></returns>
    private DateTime GetCourseTime(string courseTimeType)
    {
      string type = courseTimeType;
      DateTime returnTime =
      Convert.ToDateTime(SectionTimeService.GetEntity(u => u.F_CourseTimeType == courseTimeType).FirstOrDefault()?.F_Time);
      return returnTime;
    }

    /// <summary>
    /// 开关灯
    /// </summary>
    /// <param name="classroom"></param>
    /// <param name="nodeAdd"></param>
    /// <param name="onoff"></param>
    /// <returns></returns>
    public void SetLamp(string classroom, string nodeAdd, string onoff)
    {
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Lamp"));
      byte b;
      // byte fun = 0x01;
      b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
      PortService.SendConvertCmd(fun, classroom, nodeAdd, b);
    }
  }
  public class CourseTimeType
  {
    public const string Section1_2 = "1-2节";
    public const string Section3_4 = "3-4节";
    public const string Section5_6 = "5-6节";
    public const string Section7_8 = "7-8节";
    public const string Section9_10 = "9-10节";
    public const string Section1_4 = "1-4节";
    public const string Section5_8 = "5-8节";
  }
}