using SmartClass.Infrastructure.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace SmartClass.Models.SerialPortRelated
{
    public class SerialPortServerClient
    {

        SocketClient socketClient = null;

        public void SendData(byte[] data)
        {
            socketClient = new SocketClient();
            socketClient.ConnectAsync();
            socketClient.Send(data);
            socketClient.CloseConnect();
        }
        public byte[] SendSearchData(byte[] data)
        {
            socketClient = new SocketClient();
            socketClient.ConnectAsync();
            socketClient.SendSearchMessage(data.HexToStr());
            byte[] bs = GetReturnData();
            socketClient.CloseConnect();
            return bs;            
        }
        public byte[] SearchAlarmData(string data)
        {
            socketClient = new SocketClient();
            socketClient.ConnectAsync();
            socketClient.SendSearchMessage(data);
            byte[] bs = GetReturnData();
            socketClient.CloseConnect();
            return bs;
        }

        public byte[] GetReturnData()
        {
            byte[] data = socketClient.Recever();
            return data;
        }
    }
}