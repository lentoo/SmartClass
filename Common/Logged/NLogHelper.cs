using NLog;

namespace SmartClass.Infrastructure.Logged
{
    /// <summary>
    /// 日志记录辅助类
    /// </summary>
    public class NLogHelper:ILogHelper
    {
        private  readonly Logger Log = LogManager.GetCurrentClassLogger();
        public void Debug(object ex)
        {
            Log.Debug(ex);
        }

        public void Warn(object ex)
        {
            Log.Warn(ex);
        }

        public void Error(object ex)
        {
            Log.Error(ex);
        }

        public void Info(object ex)
        {
            Log.Info(ex);
        }
    }
}
