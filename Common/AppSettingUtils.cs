

using System.Configuration;

namespace Common
{
    public class AppSettingUtils
    {

        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}