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
        public Int32 ExceptionCount { get; set; }
        public Int32 Count { get; set; }
        public Int32 NormalCount { get; set; }
    }
}
