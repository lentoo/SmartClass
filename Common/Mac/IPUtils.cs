using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Infrastructure.Mac
{

    public class IPUtils
    {
        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <param name="addressFamily">ip类型，默认IPv4</param>
        /// <returns></returns>
        public static string GetHostAddresse(AddressFamily addressFamily=AddressFamily.InterNetwork)
        {
            string name = Dns.GetHostName();
            var ips= Dns.GetHostAddresses(name);
            return ips.FirstOrDefault(ip => ip.AddressFamily == addressFamily)?.ToString();
        }
    }
}
