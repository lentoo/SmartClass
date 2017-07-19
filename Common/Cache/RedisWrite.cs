
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Common;

namespace Common.Cache
{
    public class RedisWrite : ICacheHelper
    {
        private static readonly string[] ReadOnlyHosts = ConfigurationManager.AppSettings["RedisReadOnlyHosts"].Split(';');
        private static readonly string[] ReadWriteHosts = ConfigurationManager.AppSettings["RedisReadWriteHosts"].Split(';');
        private static PooledRedisClientManager prcm = CreateManager(ReadWriteHosts, ReadOnlyHosts);

        private static PooledRedisClientManager CreateManager(string[] readWriteHost, string[] readOnlyHost)
        {
            return new PooledRedisClientManager(readWriteHost, readOnlyHost, new RedisClientManagerConfig()
            {
                MaxReadPoolSize = 5,  // “读”链接池链接数  
                MaxWritePoolSize = 5,  // “写”链接池链接数 
                AutoStart = true
            });
        }

        //public static RedisClient rc = new RedisClient(RedisServerIP, RedisServerPort);
        public bool AddCache<T>(string key, T value)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Add(key, value);
            }
        }

        public bool AddCache<T>(string key, T value, DateTime exp)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Add(key, value);
            }
        }

        public bool DeleteCache(string key)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Remove(key);
            }
        }

        public T GetCache<T>(string key)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Get<T>(key);
            }
        }

        public bool SetCache<T>(string key, T value)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Set(key, value);
            }
        }

        public bool SetCache<T>(string key, T value, DateTime exp)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Set(key, value, exp);
            }
        }
    }
}
