
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cache
{
    public class RedisWrite : ICacheHelper
    {
        private static string RedisServerIP = ConfigurationManager.AppSettings["RedisServerIP"];
        private static int RedisServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["RedisServerPort"]);

        public static RedisClient rc = new RedisClient(RedisServerIP, RedisServerPort);
        public bool AddCache<T>(string key, T value)
        {
            return rc.Add(key, value);
        }

        public bool AddCache<T>(string key, T value, DateTime exp)
        {
            return rc.Add(key, value, exp);
        }

        public bool DeleteCache(string key)
        {
            return rc.Remove(key);
        }

        public T GetCache<T>(string key)
        {
            return rc.Get<T>(key);
        }

        public bool SetCache<T>(string key, T value)
        {
            return rc.Set(key, value);
        }

        public bool SetCache<T>(string key, T value, DateTime exp)
        {
            return rc.Set(key, value, exp);
        }
    }
}
