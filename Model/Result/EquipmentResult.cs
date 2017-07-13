using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Properties;


namespace Model
{
    /// <summary>
    /// 操作执行器
    /// </summary>
    public class EquipmentResult : ModelResult
    {
        public object AppendData { get; set; }
        public int ExceptionCount { get; set; }
        public int Count { get; set; }
        public int NormalCount { get; set; }
    }
}
