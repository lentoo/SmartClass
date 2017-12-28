using SmartClass.Infrastructure;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmartClass.Infrastructure.Extended;

namespace SmartClass.Models.SerialPortRelated
{
    public class SocketClient
    {
        private EasyClient easyClient;
        private byte[] buff;
        private CountdownEvent countdownEvent;
        private IPEndPoint point = null;
        private object lockObj = new object();
        public SocketClient()
        {
            countdownEvent= new CountdownEvent(1);            
            var address = ConfigurationManager.AppSettings.Get("remoteSerialPortServerAddress");
            var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("remoteSerialPortServerPort"));
            var remoteAddr = IPAddress.Parse(address);
            point = new IPEndPoint(remoteAddr, port);
            easyClient = new EasyClient();
            easyClient.Initialize(new MyReceiveFilter(), Request);
        }
        private void Request(StringPackageInfo obj)
        {
            string str = obj.Key;
            if (str.Contains("\r\n"))
            {
                str = str.Substring(0, str.Length - 2);
            }
            byte[] bs = str.StrToHexByte();
            buff = bs;
            countdownEvent.Signal();
        }
        public bool CloseConnect()
        {
            if (easyClient.IsConnected)
            {
                return easyClient.Close().Result;
            }
            return true;            
        }
        public bool ConnectAsync()
        {
            if (!easyClient.IsConnected)
            {
                return easyClient.ConnectAsync(point).Result;
            }
            return true;
        }
        public void Send(byte[] data)
        {
            easyClient.Send(Encoding.UTF8.GetBytes((data.HexToStr() + "\r\n")));
        }
        public byte[] Recever()
        {
            if(countdownEvent.Wait(TimeSpan.FromSeconds(3)))            
            {
                Debug.WriteLine("接收服务端到发送的数据，长度为：" +buff.Length);
            }
            else
            {
                Debug.WriteLine("接收超时");
                return null;
            }
            return buff;
        }

       

        public void SendSearchMessage(string message)
        {
            lock (lockObj)
            {
                ConnectAsync();
                easyClient.Send(Encoding.UTF8.GetBytes((message+"\r\n")));                
                Thread.Sleep(200);
            }
        }
    }
}
