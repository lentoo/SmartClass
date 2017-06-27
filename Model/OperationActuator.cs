using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 操作执行器
    /// </summary>
    public class OperationActuator
    {
        public bool Status { get; set; }
        public string AppendData { get; set; }
        public string Message { get; set; }
        public object ErrorData { get; set; }
    }
}
