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

namespace SmartClass.Models.SerialPortRelated
{
  public static class SocketClient
  {
    static Socket socketClient = null;
    static IPEndPoint point = null;
    static byte[] datas = new byte[1024];
    static object lockObj = new object();
    public static Socket Connect()
    {
      var address = ConfigurationManager.AppSettings.Get("remoteSerialPortServerAddress");
      var port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("remoteSerialPortServerPort"));
      var remoteAddr = IPAddress.Parse(address);
      socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      point = new IPEndPoint(remoteAddr, port);
      BeginConnect();
      //socketClient.Connect(point);
      return socketClient;
    }
    private static void BeginConnect()
    {
      try
      {
        Debug.WriteLine("准备连接");
        socketClient.Connect(point);
        Debug.WriteLine("连接成功");
      }
      catch (Exception ex)
      {
        Debug.WriteLine("连接失败:" + ex.Message);
        socketClient.Close();
        socketClient.Dispose();
      }
    }
    public static byte[] GetReturnData()
    {
      byte[] data = null;
      Debug.WriteLine("准备接收服务端发送的数据");
      int len = socketClient.Receive(datas);
      if (len == 1)
      {
        return null;
      }
      Debug.WriteLine("接收服务端到发送的数据，长度为：" + len);
      data = new byte[len];
      Array.Copy(datas, 0, data, 0, len);
      return data;
    }
    public static void CloseConnect()
    {
      if (socketClient != null)
      {
        socketClient.Close();
        socketClient.Dispose();
        socketClient = null;
      }
    }
    public static void SendMessage(byte[] message)
    {
      lock (lockObj)
      {
        if (socketClient == null || !socketClient.Connected)
        {
          socketClient = Connect();
        }
        Debug.WriteLine("准备发送消息");
        socketClient.Send(message);

        CloseConnect();
        Debug.WriteLine("发送消息成功");
      }
    }
    public static void SendSearchMessage(byte[] message)
    {
      lock (lockObj)
      {
        if (socketClient == null || !socketClient.Connected)
        {
          socketClient = Connect();
        }
        Debug.WriteLine("准备发送查询消息");
        socketClient.Send(message);
        Debug.WriteLine("发送查询消息成功");
        Thread.Sleep(150);
      }
    }
  }
}
