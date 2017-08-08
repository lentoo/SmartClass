using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Cache;
using Common.Exception;
using Model.Courses;

namespace BLL
{
    public partial class Z_CourseService
    {
        /// <summary>
        /// 开学时间服务对象
        /// </summary>
        public IZ_SchoolTimeService SchoolTimeService { get; set; }

        public ISys_UserService UserService { get; set; }
        public IZ_StudentService StudentService { get; set; }
        public IZ_ClassService ClassService { get; set; }
        public IZ_ProfessionService ProfessionService { get; set; }
        public IZ_GradeService GradeService { get; set; }
        public IZ_SectionTimeService SectionTimeService { get; set; }
        
        public ICacheHelper Cache { get; set; }
        /// <summary>
        /// 学生课程表获取
        /// </summary>
        /// <param name="StuNo">学生编号</param>
        /// <returns></returns>
        public List<Course> GetStudentCourse(string StuNo)
        {
            List<Course> courseList = new List<Course>();
            try
            {
                //得到该学生的信息
                Z_Student student = StudentService.GetEntity(u => u.F_StuNo == StuNo).FirstOrDefault();
                //得到该学生所在的班级
                Z_Class cClass = ClassService.GetEntity(c => c.F_Id == student.Z_C_F_Id).FirstOrDefault();
                //查询到该学生所在的年级
                Z_Grade grade = GradeService.GetEntity(g => g.F_ID == cClass.Z_G_F_ID).FirstOrDefault();
                //得到该学生所在的专业
                Z_Profession profession = ProfessionService.GetEntity(u => u.F_ID == cClass.Z_P_F_ID).FirstOrDefault();
                //得到当前是第几周
                SchollTime schollTime = GetSchollTime();

                //查询到该专业的所有课程
                IQueryable<Z_Course> courses = GetEntity(u => u.F_Major == profession.F_ProName).Where(u => u.F_Grade == grade.F_GradeName).Where(u => u.F_Class.Contains(cClass.F_ClassName)).Where(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).Where(u=>u.F_Term==schollTime.Term.ToString());
                foreach (var item in courses)
                {
                    if (int.Parse(item.F_BeginWeek) <= schollTime.Weeks && int.Parse(item.F_EndWeek) >= schollTime.Weeks)
                    {
                        Course course = new Course()
                        {
                            F_Id = item.F_Id,
                            Classes = item.F_Class,
                            CourseName = item.F_FullName,
                            F_BeginWeek = item.F_BeginWeek,
                            TeacherName = item.F_TeacherName,
                            F_EndWeek = item.F_EndWeek,
                            F_CourseTimeType = item.F_CourseTimeType,
                            F_EnCode = item.F_EnCode,
                            Major = item.F_Major,
                            F_RoomNo = item.F_RoomCode,
                            RoomName = item.F_RoomName,
                            F_Week = item.F_Week
                        };
                        courseList.Add(course);
                    }
                }
                courseList = courseList.OrderBy(u => u.F_Week).ThenBy(u => u.F_CourseTimeType).ToList();
            }
            catch (Exception e)
            {
                ExceptionHelper.AddException(e);
            }
            return courseList;
        }
        /// <summary>
        /// 老师本周的课程
        /// </summary>
        /// <param name="TeaNo">教师编号</param>
        /// <returns></returns>
        public List<Course> GetTeacherCourse(string TeaNo)
        {
            Sys_User user = UserService.GetEntity(u => u.F_Account == TeaNo).FirstOrDefault();
            SchollTime schollTime = GetSchollTime();
            //TODO 这边需要用教师编号查询，不应该用教师名称
            IQueryable<Z_Course> courses = GetEntity(u => u.F_TeacherName == user.F_RealName)
                .Where(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).Where(u => u.F_Term == schollTime.Term.ToString());
            List<Course> courseList = new List<Course>();
            foreach (var item in courses)
            {
                if (int.Parse(item.F_BeginWeek) <= schollTime.Weeks && int.Parse(item.F_EndWeek) >= schollTime.Weeks)
                {
                    Course course = new Course()
                    {
                        F_Id = item.F_Id,
                        Classes = item.F_Class,
                        CourseName = item.F_FullName,
                        F_BeginWeek = item.F_BeginWeek,
                        F_EndWeek = item.F_EndWeek,
                        TeacherName = item.F_TeacherName,
                        F_CourseTimeType = item.F_CourseTimeType,
                        F_EnCode = item.F_EnCode,
                        F_RoomNo = item.F_RoomCode,
                        Major = item.F_Major,
                        RoomName = item.F_RoomName,
                        F_Week = item.F_Week
                    };
                    courseList.Add(course);
                }
            }
            courseList= courseList.OrderBy(u => u.F_Week).ThenBy(u => u.F_CourseTimeType).ToList();
            return courseList;
        }

        public List<Z_SectionTime> GetSectionTime()
        {
            List<Z_SectionTime> list = Cache.GetCache<List<Z_SectionTime>>("SectionTime");
            list = list ?? SectionTimeService.GetEntity(u => true).ToList();
            Cache.AddCache("SectionTime", list);
            return list;
        }
        /// <summary>
        /// 得到当前的时间与开学的时间状态
        /// </summary>
        private SchollTime GetSchollTime()
        {
            //获取当前网络时间
            DateTime currenTime = Convert.ToDateTime(Common.Extended.DatetimeExtened.GetNetDateTime());
            int year = currenTime.Year;         //今天的年
            int month = currenTime.Month;       //今天的月
            int term;                          //第几学期
            if (month >= 9 || month <= 1)
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
            SchollTime schollTime = new SchollTime()
            {
                Month = month,
                Term = term,
                Weeks = weeks,
                Year = year,
                SearchYear = searchYear
            };
            return schollTime;
        }
    }

    public class SchollTime
    {
        public string SearchYear { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Term { get; set; }
        public int Weeks { get; set; }
    }
}
