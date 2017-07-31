using Models.Classes;
using System.Collections.Generic;

namespace SmartClass.Models.Classes
{
    public class Floors
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 教室集合
        /// </summary>
        public List<ClassRoom> ClassRooms { get; set; }
        /// <summary>
        /// 是否有异常设备
        /// </summary>
        public bool AbnormalEquipment { get; set; }
        /// <summary>
        /// 异常教室数量
        /// </summary>
        public int ExceptionCount { get; set; }
        public Floors()
        {
            ClassRooms = new List<ClassRoom>();
        }
    }
}