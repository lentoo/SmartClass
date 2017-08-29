using SmartClass.Infrastructure.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SmartClass.Infrastructure.Mac
{
    public class MacUtils
    {
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);
        public static string GetClientMAC(HttpRequestBase Request)
        {
            string mac_dest = string.Empty;
            // 在此处放置用户代码以初始化页面
            try
            {
                string userip = Request.UserHostAddress;
                string strClientIP = Request.UserHostAddress.ToString().Trim();
                Int32 ldest = inet_addr(strClientIP); //目的地的ip 
                Int32 lhost = inet_addr("");   //本地服务器的ip 
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");
                while (mac_src.Length < 12)
                {
                    mac_src = mac_src.Insert(0, "0");
                }
                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                    {
                        if (i == 10)
                        {
                            mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                        else
                        {
                            mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHelper.AddException(ex);
            }
            return mac_dest;
        }
        public static string GetClientMAC(HttpRequest Request)
        {
            string mac_dest = string.Empty;
            // 在此处放置用户代码以初始化页面
            try
            {
                string userip = Request.UserHostAddress;
                string strClientIP = Request.UserHostAddress.ToString().Trim();
                Int32 ldest = inet_addr(strClientIP); //目的地的ip 
                Int32 lhost = inet_addr("");   //本地服务器的ip 
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");
                while (mac_src.Length < 12)
                {
                    mac_src = mac_src.Insert(0, "0");
                }
                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                    {
                        if (i == 10)
                        {
                            mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                        else
                        {
                            mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHelper.AddException(ex);
            }
            return mac_dest;
        }
    }
}
