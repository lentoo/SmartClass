
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cache
{
    public class MemcacheHelper : ICacheHelper
    {
        public MemcachedClient mc = MemCached.getInstance();
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public bool AddCache<T>(string key, T value)
        {
            return mc.Store(StoreMode.Add, key, value);
        }
        /// <summary>
        /// 添加缓存，并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public bool AddCache<T>(string key, T value, DateTime exp)
        {
            return mc.Store(Enyim.Caching.Memcached.StoreMode.Add, key, value, exp);
        }
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetCache<T>(string key, T value)
        {
            return mc.Store(Enyim.Caching.Memcached.StoreMode.Set, key, value);
        }
        /// <summary>
        /// 修改缓存并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public bool SetCache<T>(string key, T value, DateTime exp)
        {
            return mc.Store(Enyim.Caching.Memcached.StoreMode.Set, key, value, exp);
        }
        /// <summary>
        /// 通过键获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key)
        {
            return mc.Get<T>(key);
        }
        /// <summary>
        /// 通过键删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool DeleteCache(string key)
        {
            return mc.Remove(key);
        }
    }
    public sealed class MemCached
    {
        private static MemcachedClient MemClient { get; set; }
        static readonly object padlock = new object();
        //线程安全的单例模式  
        public static MemcachedClient getInstance()
        {
            if (MemClient == null)
            {
                lock (padlock)
                {
                    if (MemClient == null)
                    {
                        MemClientInit();
                    }
                }
            }
            return MemClient;
        }

        private static void MemClientInit()
        {
            string serverList = ConfigurationManager.AppSettings["CacheServerList"].ToString();
            string[] serverlist = serverList.Split(',');
            //初始化缓存  
            MemcachedClientConfiguration memConfig = new MemcachedClientConfiguration();
            // 配置文件 - ip  
            memConfig.AddServer(serverlist[0]);
            // 配置文件 - 协议  
            memConfig.Protocol = MemcachedProtocol.Binary;
            //下面请根据实例的最大连接数进行设置  
            memConfig.SocketPool.MinPoolSize = 5;
            memConfig.SocketPool.MaxPoolSize = 200;
            MemClient = new MemcachedClient(memConfig);
        }
    }
}
