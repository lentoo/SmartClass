using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IBLL;
using Model;
using SmartClass.Models.Course;

namespace SmartClass.Controllers
{
    public class CourseController : Controller
    {
        public IZ_CourseService CourseService { get; set; }
        public IZ_RoomService RoomService { get; set; }

        public ActionResult GetToDayCourse()
        {
            DateTime currenTime = DateTime.Today;
            int year = currenTime.Year;         //今天的年
            int month = currenTime.Month;       //今天的月
            int day = currenTime.Day;           //今天的日
            string week = ((float)currenTime.DayOfWeek).ToString();    //今天星期几
            int term;                          //第几学期
            if (month >= 9 && month <= 2)
            {
                term = 1;
            }
            else
            {
                term = 2;
            }
            string searchYear = term == 1 ? year + "-" : "-" + year;
            DateTime schoolTime = new DateTime(2017, 2, 18);  //开学时间
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
            //var rooms = RoomService.GetEntity(r => toDayCourses.Any(c => c.F_RoomCode == r.F_EnCode)).ToList();
            
            return Json(new
            {
                toDayCourses
            }, JsonRequestBehavior.AllowGet);

        }
    }
}