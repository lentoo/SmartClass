using SmartClass.IService;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartClass.Infrastructure.Cache;
using SmartClass.Infrastructure.Exception;
using Model.DTO.Courses;
using System.Globalization;
using System.Data.Objects.SqlClient;
using System.Text;
using Model.AutoMapperConfig;
using System.Threading.Tasks;

namespace SmartClass.Service
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
    public IZ_RoomService RoomService { get; set; }

    public ICacheHelper Cache { get; set; }
    /// <summary>
    /// 学生的课程
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
        IQueryable<Z_Course> courses = GetEntity(u => u.F_Major == profession.F_ProName).Where(u => u.F_Grade == grade.F_GradeName).Where(u => u.F_Class.Contains(cClass.F_ClassName)).Where(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).Where(u => u.F_Term == schollTime.Term.ToString());

        var cl = courses.OrderBy(u => u.F_Week).ThenBy(u => u.F_CourseTimeType).ToList();
        cl.ForEach(u => u.F_CourseTimeType = u.F_CourseTimeType.Substring(0, u.F_CourseTimeType.Length - 1));
        courseList = AutoMapperConfig.MapList(cl, courseList);
        ComputedTimeLength(courseList);
      }
      catch (Exception e)
      {
        throw e;
      }
      return courseList;
    }
    /// <summary>
    /// 老师的课程
    /// </summary>
    /// <param name="TeaNo">教师编号</param>
    /// <returns></returns>
    public List<Course> GetTeacherCourse(string TeaNo)
    {
      Sys_User user = UserService.GetEntity(u => u.F_Account == TeaNo).FirstOrDefault();
      SchollTime schollTime = GetSchollTime();
      //TODO 这边需要用教师编号查询，不应该用教师名称
      //查询老师当前学年学期的所有课程
      IQueryable<Z_Course> courses = GetEntity(u => u.F_TeacherName == user.F_RealName)
          .Where(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).Where(u => u.F_Term == schollTime.Term.ToString());
      List<Course> courseList = new List<Course>();

      //对课程按周，课时类型升序排序
      var cl = courses.OrderBy(u => u.F_Week).ThenBy(u => u.F_CourseTimeType).ToList();
      cl.ForEach(u => u.F_CourseTimeType = u.F_CourseTimeType.Substring(0, u.F_CourseTimeType.Length - 1));
      courseList = AutoMapperConfig.MapList(cl, courseList);
      ComputedTimeLength(courseList);
      return courseList;
    }
    /// <summary>
    /// 选出在当前周的课程
    /// </summary>
    /// <param name="courses"></param>
    /// <param name="schollTime"></param>
    /// <returns></returns>
    public List<Course> SelectCourseInTheCurrentWeek(List<Course> courses, SchollTime schollTime)
    {
      List<Course> courseList = new List<Course>();
      foreach (var item in courses)
      {
        //筛选出开始周和结束周在当前周范围内的课程
        if (int.Parse(item.BeginWeek) <= schollTime.Weeks && int.Parse(item.EndWeek) >= schollTime.Weeks)
        {
          Course course = AutoMapperConfig.Map<Course>(item);
          courseList.Add(course);
        }
      }
      return courseList;
    }

    /// <summary>
    /// 获取课时类型
    /// </summary>
    /// <returns></returns>
    public List<Z_SectionTime> GetSectionTime()
    {
      //TODO 上线后改成从缓存获取
      List<Z_SectionTime> list = Cache.GetCache<List<Z_SectionTime>>("SectionTime");
      if (list == null)
      {
        list = SectionTimeService.GetEntity(u => true).ToList();
        Cache.AddCache("SectionTime", list, DateTime.Now.AddDays(7));
      }
      return list;
    }

    /// <summary>
    /// 得到当前的时间与开学的时间状态
    /// </summary>
    public SchollTime GetSchollTime()
    {
      //获取当前网络时间
      DateTime currenTime = Convert.ToDateTime(Infrastructure.Extended.DatetimeExtened.GetNetDateTime());
      int year = currenTime.Year;         //今天的年
      int month = currenTime.Month;       //今天的月
      int term;                          //第几学期
      string searchYear = string.Empty;
      if (month >= 9 || month <= 2)
      {
        term = 1;
        if (month >= 9) searchYear = year + "-" + (year + 1);
        else searchYear = "-" + year;
      }
      else
      {
        term = 2;
        searchYear = "-" + year;
      }
      //searchYear = term == 1 ? (year - 1) + "-" + year : "-" + year;
      Z_SchoolTime ZSchoolTime = SchoolTimeService.GetEntity(u => u.F_SchoolYear.Contains(searchYear)).FirstOrDefault(u => u.F_Term == term + "");
      DateTime schoolTime = ZSchoolTime.F_SchoolTime;  //开学时间
      TimeSpan span = currenTime - schoolTime;   //距离开学过去多久了
      int days = span.Days;               //距离开学过去几天了
      int weeks = Convert.ToInt32(Math.Ceiling(days / 7.0)); //开学第几周了
      string week = ((float)currenTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
      weeks = weeks == 0 ? 1 : weeks;
      SchollTime schollTime = new SchollTime()
      {
        Week = week,
        Month = month,
        Term = term,
        Weeks = weeks,
        Year = year,
        SearchYear = searchYear,
        CurrentTime = currenTime
      };
      return schollTime;
    }

    /// <summary>
    /// 根据课室ID获取在该课室上课的所有课程
    /// </summary>
    /// <param name="roomId">课室ID</param>
    /// <returns></returns>
    public List<Course> GetCoursesForRoomId(string roomId)
    {
      SchollTime schollTime = GetSchollTime();
      IQueryable<Z_Course> courses = GetEntity(u => u.F_RoomCode == roomId)
          .Where(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).Where(u => u.F_Term == schollTime.Term.ToString());
      List<Course> courseList = new List<Course>();
      foreach (var item in courses)
      {
        //筛选出开始周和结束周在当前周范围内的课程
        if (int.Parse(item.F_BeginWeek) <= schollTime.Weeks && int.Parse(item.F_EndWeek) >= schollTime.Weeks)
        {
          Course course = AutoMapperConfig.Map<Course>(item);
          courseList.Add(course);
        }
      }
      //对课程按周，课时类型升序排序
      courseList = courseList.OrderBy(u => u.Week).ThenBy(u => u.CourseTimeType).ToList();
      return courseList;
    }

    /// <summary>
    /// 获取今日所有课程
    /// </summary>
    /// <returns></returns>
    public List<Course> GetToDayCourse()
    {
      SchollTime schollTime = GetSchollTime();
      Z_SchoolTime ZSchoolTime = SchoolTimeService.GetEntity(u => u.F_SchoolYear.Contains(schollTime.SearchYear)).FirstOrDefault(u => u.F_Term == schollTime.Term + "");
      DateTime schoolTime = ZSchoolTime.F_SchoolTime;  //开学时间
      DateTime endTime = ZSchoolTime.F_EndTime; //该学期结束时间
      if (schollTime.CurrentTime >= endTime)
      {
        return new List<Course>();
      }
      TimeSpan span = schollTime.CurrentTime - schoolTime;   //距离开学过去多久了
      int days = span.Days;               //距离开学过去几天了
      int weeks = Convert.ToInt32(Math.Ceiling(days / 7.0)); //开学第几周了

      var zCourses = GetEntity(u => u.F_SchoolYear.Contains(schollTime.SearchYear) && u.F_Term == schollTime.Term.ToString() && u.F_Week == schollTime.Week);  //获取符合条件的课程信息

      var rooms = RoomService.GetEntity(r => true);
      IQueryable<Course> list = (from r in rooms
                                 join course in zCourses on r.F_EnCode equals course.F_RoomCode
                                 select new Course()
                                 {
                                   Id = course.F_Id,
                                   TeacherName = course.F_TeacherName,
                                   Week = course.F_Week,
                                   CourseName = course.F_FullName,
                                   Grade = course.F_Grade,
                                   Major = course.F_Major,
                                   Classes = course.F_Class,
                                   RoomName = course.F_RoomName,
                                   RoomNo = r.F_RoomNo,
                                   CourseTimeType = course.F_CourseTimeType,
                                   EnCode = course.F_EnCode,
                                   BeginWeek = course.F_BeginWeek,
                                   EndWeek = course.F_EndWeek
                                 });

      var toDayCourses = new List<Course>();
      toDayCourses = SelectCourseInTheCurrentWeek(toDayCourses, schollTime);
      return toDayCourses;
    }

    /// <summary>
    /// 计算课程时长
    /// </summary>
    /// <param name="courseList"></param>
    private void ComputedTimeLength(List<Course> courseList)
    {
      courseList.ForEach(course =>
      {
        var courseTimeType = course.CourseTimeType;
        if (courseTimeType.Contains("-"))
        {
          string[] types = courseTimeType.Split('-');
          if (types.Length >= 2)
          {
            course.TimeLength = 1 + Convert.ToInt32(types[1]) - Convert.ToInt32(types[0]);
          }
        }
        else
        {
          course.TimeLength = 1;
        }
      });
    }
  }
}
