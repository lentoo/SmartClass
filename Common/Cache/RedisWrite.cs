
using ServiceStack.Redis;
using System;
using System.Configuration;
using SmartClass.Infrastructure.Exception;

namespace SmartClass.Infrastructure.Cache
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
            try
            {
                using (IRedisClient redis = prcm.GetClient())
                {
                    return redis.Add(key, value);
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHelper.AddException(ex);
                return false;
            }
        }

        public bool DeleteCache(string key)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                return redis.Remove(key);
            }
        }
        /// <summary>
        /// 删除缓存中某个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool DeleteCache<T>(T value)
        {
            using (IRedisClient redis = prcm.GetClient())
            {
                bool isOk=false;
                foreach (var item in redis.GetAllKeys())
                {
                    if (redis.Get<T>(item).Equals( value))
                    {
                        isOk=redis.Remove(item);
                    }
                }
                return isOk;
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
