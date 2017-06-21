using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cache
{
    public interface ICacheHelper
    {
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool AddCache(string key, object value);
        /// <summary>
        /// 添加缓存，并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        bool AddCache(string key, object value, DateTime exp);
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetCache(string key, object value);
        /// <summary>
        /// 修改缓存并指定过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        bool SetCache(string key, object value, DateTime exp);
        /// <summary>
        /// 通过键获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetCache(string key);
        /// <summary>
        /// 通过键删除缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool DeleteCache(string key);
    }
}
