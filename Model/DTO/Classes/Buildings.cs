using System.Collections.Generic;

namespace Model.DTO.Classes
{
    public class Buildings
    {
        /// <summary>
        /// 楼栋名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 楼层
        /// </summary>
        public List<Floors> Floors { get; set; }
        /// <summary>
        /// 是否有异常设备
        /// </summary>
        public bool AbnormalEquipment { get; set; }
        /// <summary>
        /// 异常教室数量
        /// </summary>
        public int ExceptionCount { get; set; }
        public Buildings()
        {
            Floors = new List<Floors>();
        }
    }
}