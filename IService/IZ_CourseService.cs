using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.DTO.Courses;

namespace SmartClass.IService
{
  public partial interface IZ_CourseService
  {
    /// <summary>
    /// 通过学生编号查询该学生的课程
    /// </summary>
    /// <param name="StuNo"></param>
    /// <returns></returns>
    List<Course> GetStudentCourse(string StuNo);
    /// <summary>
    /// 选出在当前周的课程
    /// </summary>
    /// <param name="courses"></param>
    /// <param name="schollTime"></param>
    /// <returns></returns>
    List<Course> SelectCourseInTheCurrentWeek(List<Course> courses, SchollTime schollTime);
    /// <summary>
    /// 得到指定时间或者当前的时间与开学的时间状态
    /// </summary>
    SchollTime GetSchollTime(string time=null);
    /// <summary>
    /// 通过教师编号查询该教师的课程
    /// </summary>
    /// <param name="teacherNum">教师编号</param>
    /// <returns></returns>
    List<Course> GetTeacherCourse(string TeaNo);
    /// <summary>
    /// 获取今日或指定日期下所有课程
    /// </summary>
    /// <param name="time">指定日期</param>
    /// <returns></returns>
    List<Course> GetToDayCourseOrByDate(SchollTime schollTime=null);
    /// <summary>
    /// 获取课时类型信息
    /// </summary>
    /// <returns></returns>
    List<Z_SectionTime> GetSectionTime();

    /// <summary>
    /// 根据课室ID获取在该课室上课的所有课程
    /// </summary>
    /// <param name="roomId">课室ID</param>
    /// <returns></returns>
    List<Course> GetCoursesForRoomId(string roomId);

    /// <summary>
    /// 根据课程获取该课程下的所有学生
    /// </summary>
    /// <returns></returns>
    List<Z_Student> GetStudentsByCourseId(Course course);
  }
}
