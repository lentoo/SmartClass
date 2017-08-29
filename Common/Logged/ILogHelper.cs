namespace SmartClass.Infrastructure.Logged
{
    public interface ILogHelper
    {
        void Debug(object ex);

        void Warn(object ex);

        void Error(object ex);

        void Info(object ex);
    }
}
