using System;

namespace Model.Result
{
    /// <summary>
    /// 操作执行器结果
    /// </summary>
    public class EquipmentResult : ModelResult
    {
        public object AppendData { get; set; }
        public Int32 ExceptionCount { get; set; }
        public Int32 Count { get; set; }
        public Int32 NormalCount { get; set; }
    }
}
