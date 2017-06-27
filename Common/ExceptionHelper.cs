using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ExceptionHelper
    {
        public static Queue<Exception> ExceptionQueue = new Queue<Exception>();
        public static void AddException(Exception e)
        {
            ExceptionQueue.Enqueue(e);
        }
    }
}
