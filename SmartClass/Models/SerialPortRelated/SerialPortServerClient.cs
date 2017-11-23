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
    /// <summary>
    /// 是否发送或获取到数据后关闭socket连接
    /// </summary>
    public bool IsSendClosedConnection = true;
    //SocketClient socketClient = null;
    //public SocketClient socketClient { get; set; }
    public SerialPortServerClient()
    {
      //socketClient = new SocketClient();
      // socketClient.BeginConnect();
    }
    public void SendData(byte[] data)
    {
      SocketClient.SendMessage(data);
      if (IsSendClosedConnection)
      {
        CloseConnect();
      }
    }
    public void SendSearchData(byte[] data)
    {
      SocketClient.SendSearchMessage(data);
    }
    public byte[] GetReturnData()
    {
      byte[] data = SocketClient.GetReturnData();
      if (IsSendClosedConnection)
      {
        CloseConnect();
      }
      return data;
    }
    public void CloseConnect()
    {
      SocketClient.CloseConnect();
    }
  }
}