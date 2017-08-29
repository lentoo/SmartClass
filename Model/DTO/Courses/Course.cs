namespace Model.DTO.Courses
{
    public class Course
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 课时类型
        /// </summary>
        public string F_CourseTimeType { get; set; }
        /// <summary>
        /// 开始周
        /// </summary>
        public string F_BeginWeek { get; set; }
        /// <summary>
        /// 结束周
        /// </summary>
        public string F_EndWeek { get; set; }
        /// <summary>
        /// 课程编码
        /// </summary>
        public string F_EnCode { get; set; }
        /// <summary>
        /// 教室编码
        /// </summary>
        public string F_RoomNo { get; set; }
        /// <summary>
        /// 周
        /// </summary>
        public string F_Week { get; set; }
        /// <summary>
        /// 教师名称
        /// </summary>
        public string TeacherName { get; set; }
        /// <summary>
        /// 课程名称
        /// </summary>
        public string CourseName { get; set; }
        /// <summary>
        /// 年级
        /// </summary>
        public string Grade { get; set; }
        /// <summary>
        /// 专业
        /// </summary>
        public string Major { get; set; }
        /// <summary>
        /// 班级
        /// </summary>
        public string Classes { get; set; }
        /// <summary>
        /// 教室名称
        /// </summary>
        public string RoomName { get; set; }
    }
}