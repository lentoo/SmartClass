using Autofac;
using Autofac.Integration.Mvc;
using System;

namespace Common.Cache
{
    public class CacheHelper
    {
        private static readonly string SelectCache =AppSettingUtils.GetValue("selectCache");
        private static readonly ICacheHelper Cache = AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<ICacheHelper>(SelectCache);
        private static readonly object Cachelock = new object();
        public static void AddCache<T>(string key, T value)
        {
            lock (Cachelock)
            {
                Cache.AddCache(key, value);
            }
        }

        public static void AddCache<T>(string key, T value, DateTime exp)
        {
            lock (Cachelock)
            {
                Cache.AddCache(key, value, exp);
            }

        }

        public static T GetCache<T>(string key)
        {
            lock (Cachelock)
            {
                return Cache.GetCache<T>(key);
            }
        }

        public static void SetCache<T>(string key, T value, DateTime exp)
        {
            lock (Cachelock)
            {
                Cache.SetCache(key, value, exp);
            }
        }

        public static void SetCache<T>(string key, T value)
        {
            lock (Cachelock)
            {
                Cache.SetCache(key, value);
            }
        }

        public static bool DeleteCache(string key)
        {
            lock (Cachelock)
            {
                return Cache.DeleteCache(key);
            }
        }
    }
}
