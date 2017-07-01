using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedisDemo
{
    class Program
    {
        static RedisClient redisClient = new RedisClient("192.168.162.20", 6379);
        static RedisClient redisClient1 = new RedisClient("192.168.162.20", 6379);
        static void Main(string[] args)
        {
            redisClient.Add("city", "zd");
           
            //redisClient.DequeueItemFromList("ExceptionList").Remove(0, redisClient.DequeueItemFromList("ExceptionList").Length);
            //for (int i = 0; i < 10; i++)
            //{
            //    redisClient.EnqueueItemOnList("ExceptionList", i+"张三");
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine(redisClient1.DequeueItemFromList("ExceptionList"));
            //}
            redisClient.Remove("city");
            string city = redisClient.Get<string>("city");
            Console.WriteLine(city);
            Console.ReadKey();
        }
    }
}
