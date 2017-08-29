using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    /// <summary>
    /// 控制参数
    /// </summary>
    public class ControlParams
    {
        /// <summary>
        /// 教室地址
        /// </summary>
        [Required]
        public string classroom { get; set; } = "0000";

        /// <summary>
        /// 设备节点地址
        /// </summary>
        [Required]
        public string nodeAdd { get; set; } = "00";

        /// <summary>
        /// 开关指令
        /// </summary>
        [Required]
        public string onoff { get; set; } = "00";
    }
}
