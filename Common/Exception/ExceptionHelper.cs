using System.Collections.Generic;

namespace SmartClass.Infrastructure.Exception
{
    public class ExceptionHelper
    {
        /// <summary>
        /// 异常队列
        /// </summary>
        public static Queue<System.Exception> ExceptionQueue = new Queue<System.Exception>();
        //public static RedisClient ExceptionQueue =SmartClass.Infrastructure.Cache.RedisWrite.rc;
        /// <summary>
        /// 在异常队列中添加一个异常信息
        /// </summary>
        /// <param name="e"></param>
        public static void AddException(System.Exception e)
        {
            //ExceptionQueue.Enqueue(e);
            ExceptionQueue.Enqueue(e);
        }
    }
}
