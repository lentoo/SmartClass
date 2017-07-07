using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Common.Cache
{
    public class CacheHelper
    {
        public static string selectCache = ConfigurationManager.AppSettings["selectCache"];
        public static ICacheHelper Cache = AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<ICacheHelper>(selectCache);
        private static object cachelock = new object();
        public static void AddCache<T>(string key, T value)
        {
            lock (cachelock)
            {
                Cache.AddCache(key, value);
            }
        }

        public static void AddCache<T>(string key, T value, DateTime exp)
        {
            lock (cachelock)
            {
                Cache.AddCache(key, value, exp);
            }

        }

        public static T GetCache<T>(string key)
        {
            lock (cachelock)
            {
                return Cache.GetCache<T>(key);
            }
        }

        public static void SetCache<T>(string key, T value, DateTime exp)
        {
            lock (cachelock)
            {
                Cache.SetCache(key, value, exp);
            }
        }

        public static void SetCache<T>(string key, T value)
        {
            lock (cachelock)
            {
                Cache.SetCache(key, value);
            }
        }
    }
}
