using System.Collections.Generic;

namespace Model.Properties
{
    /// <summary>
    /// 教室类
    /// </summary>
    public class ClassRoom
    {
        /// <summary>
        /// 教室ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 教室名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 所属学院名称
        /// </summary>
        public string CollegeName { get; set; }
        /// <summary>
        /// 层
        /// </summary>
        public string LayerName { get; set; }
        /// <summary>
        /// 教室编码
        /// </summary>
        public string ClassNo { get; set; }
        /// <summary>
        /// 教室中的所有传感器
        /// </summary>
        public List<SonserBase> SonserList { get; set; }
    }
}