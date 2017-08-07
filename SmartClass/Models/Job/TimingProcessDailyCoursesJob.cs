using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using IBLL;
using Model;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Common;
using SmartClass.Models.Types;
using Common.Exception;
using Model.Courses;

namespace SmartClass.Models.Job
{
    /// <summary>
    /// 定时处理每日课程表
    /// </summary>
    public class TimingProcessDailyCoursesJob : IJob
    {
        private readonly IZ_CourseService CourseService;
        private readonly IZ_RoomService RoomService;
        private readonly IZ_SchoolTimeService SchoolTimeService;
        private readonly IZ_SectionTimeService SectionTimeService;
        private readonly SerialPortService PortService;
        private DateTime time12;
        private DateTime time34;
        private DateTime time56;
        private DateTime time78;
        private DateTime time910;
        private DateTime time14;
        private DateTime time58;
        public TimingProcessDailyCoursesJob(IZ_CourseService CourseService, IZ_RoomService RoomService, IZ_SchoolTimeService SchoolTimeService, IZ_SectionTimeService SectionTimeService, SerialPortService PortService)
        {
            this.CourseService = CourseService;
            this.RoomService = RoomService;
            this.SchoolTimeService = SchoolTimeService;
            this.SectionTimeService = SectionTimeService;
            this.PortService = PortService;
        }
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                List<Z_SectionTime> sectionTimes = SectionTimeService.GetSectionTime();
                List<Course> courses = GetToDayCourse();
                Debug.WriteLine("获取成功...");
                Debug.WriteLine($"今日有{courses.Count}节课");
                Debug.WriteLine("正在处理定时任务......");
                InitSectionTime(sectionTimes);
                ProcessCourseAsync(courses);
            }
            catch (Exception ex)
            {
                ExceptionHelper.AddException(ex);
            }
        }
        /// <summary>
        /// 获取今日课程信息
        /// </summary>
        /// <returns></returns>
        public List<Course> GetToDayCourse()
        {
            DateTime currenTime = DateTime.Today;
            int year = currenTime.Year;         //今天的年
            int month = currenTime.Month;       //今天的月
            int day = currenTime.Day;           //今天的日
            string week = ((float)currenTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);    //今天星期几
            int term;                          //第几学期
            if (month >= 9 && month <= 2)
            {
                term = 1;
            }
            else
            {
                term = 2;
            }
            string searchYear = term == 1 ? (year - 1) + "-" + year : "-" + year;
            Z_SchoolTime ZSchoolTime = SchoolTimeService.GetEntity(u => u.F_SchoolYear.Contains(searchYear)).FirstOrDefault(u => u.F_Term == term + "");
            DateTime schoolTime = ZSchoolTime.F_SchoolTime;  //开学时间

            TimeSpan span = currenTime - schoolTime;   //距离开学过去多久了
            int days = span.Days;               //距离开学过去几天了
            int weeks = Convert.ToInt32(Math.Ceiling(days / 7.0)); //开学第几周了

            var zCourses = CourseService.GetEntity(u => u.F_SchoolYear.Contains(searchYear) && u.F_Term == term.ToString() && u.F_Week == week);  //获取符合条件的课程信息

            var rooms = RoomService.GetEntity(r => true);
            List<Course> list = (from r in rooms
                                 join course in zCourses on r.F_EnCode equals course.F_RoomCode
                                 select new Course()
                                 {
                                     F_RoomNo = r.F_RoomNo,
                                     F_CourseTimeType = course.F_CourseTimeType,
                                     F_EnCode = r.F_EnCode,
                                     F_BeginWeek = course.F_BeginWeek,
                                     F_EndWeek = course.F_EndWeek
                                 }).ToList();

            List<Course> toDayCourses = new List<Course>();

            foreach (Course course in list)
            {
                if (Convert.ToInt32(course.F_BeginWeek) <= weeks && Convert.ToInt32(course.F_EndWeek) >= weeks)
                {
                    toDayCourses.Add(course);
                }
            }
            return toDayCourses;
        }

        /// <summary>
        /// 初始化节次时间
        /// </summary>
        /// <param name="Times"></param>
        public void InitSectionTime(List<Z_SectionTime> Times)
        {
            DateTime today = DateTime.Today;

            foreach (var time in Times)
            {
                DateTime t = Convert.ToDateTime(time.F_Time);
                switch (time.F_CourseTimeType)
                {
                    case CourseTimeType.Section1_2:
                        time12 = t;
                        break;
                    case CourseTimeType.Section3_4:
                        time34 = t;
                        break;
                    case CourseTimeType.Section5_6:
                        time56 = t;
                        break;
                    case CourseTimeType.Section7_8:
                        time78 = t;
                        break;
                    case CourseTimeType.Section9_10:
                        time910 = t;
                        break;
                    case CourseTimeType.Section1_4:
                        time14 = t;
                        break;
                    case CourseTimeType.Section5_8:
                        time58 = t;
                        break;
                }
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
                DateTime openTime = GetCourseTime(course.F_CourseTimeType);
                openTime = openTime.AddMinutes(-10);  //提前10分钟打开
                DateTime closeTime = GetClassOver(course, toDayCourses, openTime);
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    while (true)
                    {
                        DateTime currenTime = DateTime.Now;
                        if (currenTime.Hour > openTime.Hour)//过了打开时间
                        {
                            Debug.WriteLine("过了打开时间");
                            break;
                        }
                        else if (currenTime.Hour == openTime.Hour && currenTime.Minute > openTime.Minute)
                        {
                            Debug.WriteLine("过了打开时间");
                            break;
                        }
                        if (currenTime.Hour == openTime.Hour && currenTime.Minute == openTime.Minute) //开启上课命令
                        {
                            //发送上课命令
                            //Task<string> task = SendCmd(course.F_RoomNo, "18", "open");
                            SetLamp(course.F_RoomNo, "3", "open");
                            Debug.WriteLine($"{course.F_RoomNo}课室开始上课..");
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
                            toDayCourses.Remove(course);
                            break;
                        }
                        else if (currenTime.Hour == closeTime.Hour && currenTime.Minute > closeTime.Minute)
                        {
                            Debug.WriteLine("过了关闭时间");
                            toDayCourses.Remove(course);
                            break;
                        }
                        if (currenTime.Hour == closeTime.Hour && currenTime.Minute == closeTime.Minute) //开启下课命令
                        {
                            //发送下课命令
                            //Task<string> task1 = SendCmd(course.F_RoomNo, "18", "close");
                            SetLamp(course.F_RoomNo, "18", "close");
                            Debug.WriteLine($"{course.F_RoomNo}课室下课..");
                            toDayCourses.Remove(course);
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
        /// <param name="course"></param>
        /// <param name="toDayCourses"></param>
        /// <param name="openTime"></param>
        /// <returns></returns>
        public DateTime GetClassOver(Course course, List<Course> toDayCourses, DateTime openTime)
        {
            if (course == null) throw new ArgumentNullException("Course为null");
            if (toDayCourses == null) throw new ArgumentNullException("toDayCourses为null");
            if (course.F_CourseTimeType == CourseTimeType.Section1_2 || course.F_CourseTimeType == CourseTimeType.Section5_6)
            {
                foreach (var item in toDayCourses)
                {
                    if (course.F_RoomNo == item.F_RoomNo)
                    {
                        if (course.F_CourseTimeType == CourseTimeType.Section1_2 && item.F_CourseTimeType == CourseTimeType.Section3_4)
                        {
                            return openTime.AddHours(4);
                        }
                        else if (course.F_CourseTimeType == CourseTimeType.Section5_6 && item.F_CourseTimeType == CourseTimeType.Section7_8)
                        {
                            return openTime.AddHours(4);
                        }
                    }
                }
                return openTime.AddHours(2);
            }
            else if (course.F_CourseTimeType == CourseTimeType.Section1_4 || course.F_CourseTimeType == CourseTimeType.Section5_8)
            {
                return openTime.AddHours(4);
            }
            else
            {
                return openTime.AddHours(2);
            }
        }
        /// <summary>
        /// 获取上课时间
        /// </summary>
        /// <param name="courseTimeType"></param>
        /// <returns></returns>
        private DateTime GetCourseTime(string courseTimeType)
        {
            string type = courseTimeType;
            DateTime today = DateTime.Today;
            DateTime returnTime;
            switch (type)
            {
                case CourseTimeType.Section1_2:
                    returnTime = time12;
                    break;
                case CourseTimeType.Section3_4:
                    returnTime = time34;
                    break;
                case CourseTimeType.Section5_6:
                    returnTime = time56;
                    break;
                case CourseTimeType.Section7_8:
                    returnTime = time78;
                    break;
                case CourseTimeType.Section9_10:
                    returnTime = time910;
                    break;
                case CourseTimeType.Section1_4:
                    returnTime = time14;
                    break;
                case CourseTimeType.Section5_8:
                    returnTime = time58;
                    break;
                default:
                    returnTime = DateTime.Now;
                    break;
            };
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