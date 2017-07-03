using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ModelResult
    {
        public bool Status { get; set; }
        public ResultCode ResultCode { get; set; }
        public string AppendData { get; set; }
        public string Message { get; set; }
        public object ErrorData { get; set; }
    }
}
