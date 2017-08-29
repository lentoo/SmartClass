using System;
using Model.Enum;

namespace Model.DTO.Result
{
    /// <summary>
    /// 考勤结果
    /// </summary>
    public class AttendanceResult
    {
        public ResultCode ResultCode;
        public string AttendanceId;
        public string Message;
        public string RoomNo;
        public Exception Error;
    }
}
