using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DateTimeHelper
    {
        public static long GetTotoalMilliseconds()
        {
            DateTime d1 = DateTime.Now;
            DateTime d2 = new DateTime(1970, 1, 1);
            long d = (long)d1.Subtract(d2).TotalMilliseconds;
            return d;
        }
        public static long GetTotoalMilliseconds(DateTime time)
        {            
            DateTime d2 = new DateTime(1970, 1, 1);
            long d = (long)time.Subtract(d2).TotalMilliseconds;
            return d;
        }
    }
}
