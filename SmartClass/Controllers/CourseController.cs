using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using Model;
using Model.DTO.Courses;

namespace SmartClass.Controllers
{
  /// <summary>
  /// 课程信息 控制器
  /// </summary>
  public class CourseController : Controller
  {
    public IZ_CourseService CourseService { get; set; }

    public IZ_SectionTimeService SectionTimeService { get; set; }

    public ICacheHelper Cache { get; set; }

    public ActionResult GetToDayCourse()
    {
      var toDayCourses = CourseService.GetToDayCourse();
      return Json(new
      {
        toDayCourses
      }, JsonRequestBehavior.AllowGet);
    }


    /// <summary>
    /// 获取节次上课时间
    /// </summary>
    /// <returns></returns>
    public ActionResult GetSectionTime()
    {
      var list = CourseService.GetSectionTime();
      return Json(list, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// 获取学生的课程表
    /// </summary>
    /// <param name="StuNo">学生编号</param>
    /// <returns></returns>
    public ActionResult GetStudentCourse(string StuNo)
    {
      List<Course> list = CourseService.GetStudentCourse(StuNo);
      return Json(list, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// 获取教师的课程表
    /// </summary>
    /// <param name="TeaNo"></param>
    /// <returns></returns>
    public ActionResult GetTeacherCourse(string TeaNo)
    {
      List<Course> list = CourseService.GetTeacherCourse(TeaNo);
      return Json(list, JsonRequestBehavior.AllowGet);
    }
  }
}