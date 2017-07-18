using System;
using System.Text;

namespace Common
{
    public class Base64Helper
    {

        public static string Encry(string text)
        {
            byte[] bs = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bs);
        }
        /// <summary>
        /// base64加密
        /// </summary>
        /// <param name="obj">加密对象</param>
        /// <returns></returns>
        public static string Encry(object obj)
        {
            string text = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            byte[] bs = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bs);
        }
        /// <summary>
        /// base64解密
        /// </summary>
        /// <param name="text">需要解密的字符串</param>
        /// <returns></returns>
        public static T Decry<T>(string text)
        {
            byte[] b = Convert.FromBase64String(text);
            string st = Encoding.UTF8.GetString(b);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(st);
        }
        public static string Decry(string text)
        {
            byte[] b = Convert.FromBase64String(text);
            string st = Encoding.UTF8.GetString(b);
            return st;
        }
    }
}
