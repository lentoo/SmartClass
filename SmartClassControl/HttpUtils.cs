using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartClassControl
{
    public class HttpUtils
    {
        /// <summary>
        /// Post请求带参数
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<string> CreateRequest(string Host,string parameters)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);     //创建一个请求示例
            webRequest.Method = "POST";
            webRequest.ProtocolVersion = HttpVersion.Version11;
            webRequest.ContentType = "application/x-www-form-urlencoded";            
            Stream stream = webRequest.GetRequestStream();
            byte[] data = Encoding.UTF8.GetBytes(parameters);
            stream.Write(data, 0, data.Length);
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();   //获取响应，即发送请求
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="Host"></param>
        /// <returns></returns>
        public static async Task<string> CreateRequest(string Host)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);     //创建一个请求示例
            //webRequest.Method = "POST";
            //webRequest.ProtocolVersion = HttpVersion.Version11;
            //webRequest.ContentType = "application/x-www-form-urlencoded";
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();   //获取响应，即发送请求
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// 获取所有的节次
        /// </summary>
        /// <param name="Host"></param>
        /// <returns></returns>
        public static async Task<string> GetSectionTime(string Host)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);            
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();   //获取响应，即发送请求
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }
    }
}
