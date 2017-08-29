using Model.Enum;

namespace Model.DTO.Result
{
    public class ModelResult
    {
        public bool Status { get; set; }
        public ResultCode ResultCode { get; set; }
        
        public string Message { get; set; }
        public object ErrorData { get; set; }
    }
}
