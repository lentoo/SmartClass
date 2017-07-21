using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartClassControl
{
    class Program
    {
        /// <summary>
        /// 服务器地址
        /// </summary>
        private static readonly string Host = ConfigurationManager.AppSettings["Host"];

        private static readonly string CourseAddr = ConfigurationManager.AppSettings["Course"];
        private static readonly string HaveClass = ConfigurationManager.AppSettings["HaveClass"];
        private static readonly string ApiSectionTime = ConfigurationManager.AppSettings["SectionTime"];
        //private static readonly string Access = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJBY2NvdW50IjoiYWRtaW4iLCJFeHAiOjAuMCwiSU1FSSI6bnVsbH0.aKQm1_xUTFnUpgAGQ2R0usV9Lj9UqdSXeZciBc-AZlU";
        private static DateTime time12;
        private static DateTime time34;
        private static DateTime time56;
        private static DateTime time78;
        private static DateTime time910;
        private static DateTime time14;
        private static DateTime time58;
        private static string Access_Token = "";
        static void Main(string[] args)
        {
            //ConfigurationManager.AppSettings["Host"] = "abc";
            Console.WriteLine("正在登陆，请稍后");

            Task<string> logonResult = Login();
            string result = logonResult.Result;
            Console.WriteLine("登陆成功");
            LoginResult lr = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResult>(result);
            Access_Token = lr.AppendData;
            Console.WriteLine("正在获取今天的课程信息......");
            Task<string> taskSectionTime = HttpUtils.GetSectionTime(Host + ApiSectionTime);     //获取所有的节次信息

            string strSectionTime = taskSectionTime.Result;

            List<SectionTime> sectionTimes =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<SectionTime>>(strSectionTime);
            InitSectionTime(sectionTimes);                      //初始化节次时间

            Task<string> task = GetToDayCourseAsync();          //获取今日课程信息
            string json = task.Result;

            Courses courses = Newtonsoft.Json.JsonConvert.DeserializeObject<Courses>(json);
            Console.WriteLine("获取成功...");
            Console.WriteLine($"今日有{courses.toDayCourses.Count}节课");
            Console.WriteLine("正在处理定时任务......");

            ProcessCourseAsync(courses);

            while (courses.toDayCourses.Count > 0)
            {
                //Console.ReadKey();
            }
            Console.WriteLine("今日任务处理完毕，按任意键退出程序");
            Console.ReadKey();
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <returns></returns>
        static async Task<string> Login()
        {
            string apiLogin = ConfigurationManager.AppSettings["Logon"];
            return await HttpUtils.CreateRequest(Host + apiLogin, "account=admin&Pwd=4a7d1ed414474e4033ac29ccb8653d9b&imei=0000");
        }
        /// <summary>
        /// 获取今日课程信息
        /// </summary>
        /// <returns></returns>
        static async Task<string> GetToDayCourseAsync()
        {
            return await HttpUtils.CreateRequest(Host + CourseAddr);
        }
        /// <summary>
        /// 发送上课开关命令
        /// </summary>
        /// <param name="classroom"></param>
        /// <param name="nodeAddr"></param>
        /// <param name="onoff"></param>
        /// <returns></returns>
        static async Task<string> SendCmd(string classroom, string nodeAddr, string onoff)
        {
            return await HttpUtils.CreateRequest(Host + HaveClass, $"classroom={classroom}&nodeAdd={nodeAddr}&onoff={onoff}&Access={Access_Token}");
        }
        /// <summary>
        /// 异步处理今日课程
        /// </summary>
        /// <param name="courses"></param>
        static void ProcessCourseAsync(Courses courses)
        {
            List<Course> toDayCourses = courses.toDayCourses;
            foreach (Course course in toDayCourses)
            {
                DateTime openTime = GetCourseTime(course.F_CourseTimeType);
                openTime = openTime.AddMinutes(-10);  //提前10分钟打开

                //DateTime closeTime =
                //    course.F_CourseTimeType == CourseTimeType.Section1_4
                //    ? openTime.AddHours(4)
                //    : course.F_CourseTimeType == CourseTimeType.Section5_8
                //        ? openTime.AddHours(4)
                //        : openTime.AddHours(2);
                DateTime closeTime = GetClassOver(course, toDayCourses, openTime);
                //openTime.AddMinutes(2);//.AddHours(2).AddMinutes(-1);
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    while (true)
                    {
                        DateTime currenTime = DateTime.Now;
                        if (currenTime.Hour > openTime.Hour && currenTime.Minute > openTime.Minute)//过了打开时间
                        {
                            Console.WriteLine("过了打开时间");
                            toDayCourses.Remove(course);
                            break;
                        }
                        if (currenTime.Hour == openTime.Hour && currenTime.Minute == openTime.Minute) //开启上课命令
                        {
                            //发送上课命令
                            Task<string> task = SendCmd(course.F_RoomNo, "18", "open");
                            Console.WriteLine($"{course.F_RoomNo}课室开始上课..");
                            break;
                        }
                        Thread.Sleep(30000);
                    }
                    while (true)
                    {
                        DateTime currenTime = DateTime.Now;
                        if (currenTime.Hour > closeTime.Hour && currenTime.Minute > closeTime.Minute)
                        {
                            Console.WriteLine("过了关闭时间");
                            toDayCourses.Remove(course);
                            break;
                        }
                        if (currenTime.Hour == closeTime.Hour && currenTime.Minute == closeTime.Minute) //开启下课命令
                        {
                            //发送下课命令
                            Task<string> task1 = SendCmd(course.F_RoomNo, "18", "close");
                            Console.WriteLine($"{course.F_RoomNo}课室下课..");
                            toDayCourses.Remove(course);
                            return;
                        }
                        Thread.Sleep(30000);
                    }
                });
            }
        }

        /// <summary>
        /// 获取上课时间
        /// </summary>
        /// <param name="courseTimeType"></param>
        /// <returns></returns>
        static DateTime GetCourseTime(string courseTimeType)
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
        /// 获取最佳关闭时间
        /// </summary>
        /// <param name="course"></param>
        /// <param name="toDayCourses"></param>
        /// <param name="openTime"></param>
        /// <returns></returns>
        static DateTime GetClassOver(Course course, List<Course> toDayCourses, DateTime openTime)
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
            }else if(course.F_CourseTimeType == CourseTimeType.Section1_4 || course.F_CourseTimeType == CourseTimeType.Section5_8)
            {
                return openTime.AddHours(4);
            }else
            {
                return openTime.AddHours(2);
            }
        }
        /// <summary>
        /// 初始化节次时间
        /// </summary>
        /// <param name="Times"></param>
        static void InitSectionTime(List<SectionTime> Times)
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
    }


    public class Courses
    {
        public List<Course> toDayCourses { get; set; }
    }
}

