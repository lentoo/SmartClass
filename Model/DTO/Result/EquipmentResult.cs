using System;

namespace Model.DTO.Result
{
    /// <summary>
    /// 操作执行器结果
    /// </summary>
    public class EquipmentResult : ModelResult

    {
        public object AppendData { get; set; }
        public int ExceptionCount { get; set; }
        public int Count { get; set; }
        public int NormalCount { get; set; }
    }
}
