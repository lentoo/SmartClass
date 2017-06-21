using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatetimeDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime d1 = DateTime.Now;
            DateTime d2 = new DateTime(1970, 1, 1);
            long d = (long)d1.Subtract(d2).TotalMilliseconds;
           // Console.WriteLine(new TimeSpan( DateTime.Now.Ticks).TotalMilliseconds);
            Console.WriteLine(d);
            Console.ReadKey();
        }
    }
}
