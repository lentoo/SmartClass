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
    //SocketClient socketClient = null;
    //public SocketClient socketClient { get; set; }
    public SerialPortServerClient()
    {
      //socketClient = new SocketClient();
     // socketClient.BeginConnect();
    }
    public void SendData(byte[]data)
    {
      SocketClient.SendMessage(data);
    }
    public void SendSearchData(byte[] data)
    {
      SocketClient.SendSearchMessage(data);
    }
    public byte[] GetReturnData()
    {
      return SocketClient.GetReturnData();
    }
    public void CloseConnect()
    {
      SocketClient.CloseConnect();
    }
  }
}