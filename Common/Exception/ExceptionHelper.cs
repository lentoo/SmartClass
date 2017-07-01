using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ExceptionHelper
    {
        /// <summary>
        /// 异常队列
        /// </summary>
        public static Queue<Exception> ExceptionQueue = new Queue<Exception>();
        //public static RedisClient ExceptionQueue =Common.Cache.RedisWrite.rc;
        /// <summary>
        /// 在异常队列中添加一个异常信息
        /// </summary>
        /// <param name="e"></param>
        public static void AddException(Exception e)
        {
            //ExceptionQueue.Enqueue(e);
            ExceptionQueue.Enqueue(e);
        }
        //public static string GetException()
        //{
        //    return ExceptionQueue.Dequeue;
        //}
    }
}
