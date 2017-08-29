namespace Model.DTO.Result
{
    /// <summary>
    /// 验证Token结果
    /// </summary>
    public class ValidateTokenResult:ModelResult
    {
        public Payload Payload { get; set; }
    }
}
