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

        private static readonly string Access = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJBY2NvdW50IjoiYWRtaW4iLCJFeHAiOjAuMCwiSU1FSSI6bnVsbH0.aKQm1_xUTFnUpgAGQ2R0usV9Lj9UqdSXeZciBc-AZlU";
        static void Main(string[] args)
        {
            ConfigurationManager.AppSettings["Host"] = "abc";
            Console.WriteLine("正在获取今天的课程信息......");
            Task<string> task = GetToDayCourseAsync();
            string json = task.Result;
            Courses courses = Newtonsoft.Json.JsonConvert.DeserializeObject<Courses>(json);
            Console.WriteLine("获取成功...");
            Console.WriteLine("正在处理定时任务......");
            ProcessCourse(courses);
            Console.ReadKey();

        }
        /// <summary>
        /// 获取今日课程信息
        /// </summary>
        /// <returns></returns>
        static async Task<string> GetToDayCourseAsync()
        {
            return await HttpUtils.CreateRequest(Host + CourseAddr);
        }

        static async Task<string> SendCmd(string classroom, string nodeAddr, string onoff)
        {
            return await HttpUtils.CreateRequest(Host + HaveClass + $"?classroom={classroom}&nodeAdd={nodeAddr}&onoff={onoff}&Access={Access}");
        }
        static void ProcessCourse(Courses courses)
        {
            List<Course> toDayCourses = courses.toDayCourses;
            foreach (Course course in toDayCourses)
            {
                DateTime openTime = GetCourseTime(course.F_CourseTimeType);
                DateTime colseTime = openTime.AddHours(2).AddMinutes(-1);
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    while (true)
                    {
                        DateTime currenTime = DateTime.Now;
                        if (currenTime.Hour == openTime.Hour && currenTime.Minute == openTime.Hour) //开启上课命令
                        {
                            //发送上课命令
                            Task<string> task = SendCmd("1234", "18", "open");
                            while (true)
                            {
                                if (currenTime.Hour == colseTime.Hour && currenTime.Minute == colseTime.Hour) //开启上课命令
                                {
                                    //发送下课命令
                                    Task<string> task1 = SendCmd("1234", "18", "close");
                                    return;
                                }
                                else
                                {
                                    Thread.Sleep(30000);
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(30000);
                        }
                    }
                });
            }
        }

        static DateTime GetCourseTime(string courseTimeType)
        {
            string type = courseTimeType;
            DateTime today = DateTime.Today;
            DateTime returnTime;
            switch (type)
            {
                case CourseTimeType.Section1_2:
                    returnTime = new DateTime(today.Year, today.Month, today.Day, 7, 55, 0);
                    break;
                case CourseTimeType.Section3_4:
                    returnTime = new DateTime(today.Year, today.Month, today.Day, 9, 50, 0);
                    break;
                case CourseTimeType.Section5_6:
                    returnTime = new DateTime(today.Year, today.Month, today.Day, 14, 15, 0);
                    break;
                case CourseTimeType.Section7_8:
                    returnTime = new DateTime(today.Year, today.Month, today.Day, 16, 05, 0);
                    break;
                case CourseTimeType.Section9_10:
                    returnTime = new DateTime(today.Year, today.Month, today.Day, 19, 20, 0);
                    break;
                default:
                    returnTime = DateTime.Now;
                    break;
            }
            return returnTime;
        }
    }


    public class Courses
    {
        public List<Course> toDayCourses { get; set; }
    }
}

