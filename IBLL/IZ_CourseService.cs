using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Courses;

namespace IBLL
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
        /// 通过教师编号查询该教师的课程
        /// </summary>
        /// <param name="teacherNum">教师编号</param>
        /// <returns></returns>
        List<Course> GetTeacherCourse(string TeaNo);

        List<Z_SectionTime> GetSectionTime();
    }
}
