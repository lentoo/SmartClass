

using System.Configuration;

namespace Common
{
    public class AppSettingUtils
    {
        /// <summary>
        /// 通过key获取配置文件信息
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}