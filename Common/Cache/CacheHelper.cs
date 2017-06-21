using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Common.Cache
{
    public class CacheHelper
    {
        public static ICacheHelper Cache = DependencyResolver.Current.GetService<ICacheHelper>();

        public static void AddCache(string key, object value)
        {

            Cache.AddCache(key, value);
        }

        public static void AddCache(string key, object value, DateTime exp)
        {
            Cache.AddCache(key, value, exp);
        }

        public static object GetCache(string key)
        {
            return Cache.GetCache(key);
        }

        public static void SetCache(string key, object value, DateTime exp)
        {
            Cache.SetCache(key, value, exp);
        }

        public static void SetCache(string key, object value)
        {
            Cache.SetCache(key, value);
        }
    }
}
