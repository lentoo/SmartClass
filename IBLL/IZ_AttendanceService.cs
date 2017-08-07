using Model;
using Model.Result;

namespace IBLL
{
    public partial interface IZ_AttendanceService : IBaseService<Z_Attendance>
    {
        /// <summary>
        /// 发起签到
        /// </summary>
        /// <param name="TeacherNum">教师编号</param>
        /// <param name="CourseNo">课程编号</param>
        AttendanceResult InitiatedAttendance(string TeacherNum, string CourseNo);

        /// <summary>
        /// 学生签到
        /// </summary>
        /// <param name="StuNo">学号</param>
        /// <param name="CourseNo">课程编号</param>
        /// <returns></returns>
        AttendanceResult CheckIn(string AttendanceId, string StuNo, string CourseNo);

    }
}
