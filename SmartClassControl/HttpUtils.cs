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
        public static async Task<string> CreateRequest(string Host)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);     //创建一个请求示例
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();   //获取响应，即发送请求
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            return await streamReader.ReadToEndAsync();
        }
    }
}
