using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SerialPortServer
{
  public class SocketServer
  {
    Socket socketServer;
    byte[] datas = new byte[1024];
    public SocketServer(int listenPort, int maxConnect)
    {
      socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      IPEndPoint point = new IPEndPoint(IPAddress.Any, listenPort);
      socketServer.Bind(point);
      socketServer.Listen(maxConnect);
      Console.WriteLine("开始监听");
    }
    public async Task BeginListen()
    {
      while (true)
      {
        try
        {
          Console.WriteLine("开始等待客户端连接");

          Socket socket = socketServer.Accept();
          Console.WriteLine(socket.RemoteEndPoint + "：已连接");
          await Task.Run(() =>
           {
             while (true)
             {
               try
               {
                 Console.WriteLine("准备接收数据");
                 //接收客户端发来的数据
                 socket.ReceiveTimeout = 3000;
                 int len = socket.Receive(datas);
                 if (len == 0)
                 {
                   Console.WriteLine($"客户端：{socket.RemoteEndPoint}断开连接");
                   socket.Close();
                   socket.Dispose();
                   break;
                 }
                 Console.WriteLine("接收到数据，长度为" + len);
                 //教室地址
                 string classroom = Convert.ToString(datas[2], 16) + Convert.ToString(datas[3], 16);
                 byte[] data = new byte[len];
                 Array.Copy(datas, 0, data, 0, len);

                 //处理接收到的数据
                 ProcessReceivedData(socket, classroom, data);
               }
               catch (Exception ex)
               {
                 Console.WriteLine("通信出现异常：" + ex.Message);
                 Console.WriteLine($"{socket.RemoteEndPoint}断开连接");
                 socket.Close();
                 socket.Dispose();
                 break;
               }
             }
           });
        }
        catch (Exception ex)
        {
          Console.WriteLine("出现异常:" + ex.Message);
        }
        finally
        {
          ;
        }
      }
    }
    /// <summary>
    /// 处理接收到的数据
    /// </summary>
    public void ProcessReceivedData(Socket socket, string classroom, byte[] data)
    {
      string str = Encoding.UTF8.GetString(data, 0, data.Length);
      switch (str)
      {
        case "报警数据":
          byte[] bs = null;
          if (SerialPortUtils.AlarmData.Count > 0) //有报警数据
          {
            bs = SerialPortUtils.AlarmData.Dequeue();
          }
          else
          {
            bs = new byte[] { 0 };
          }
          socket.Send(bs);
          break;
        default:  //默认是查询向串口查询教室设备数据
          //发送数据
          if (data.Length > 4)
          {
            if (data[4] == 0x1f)  //接收到的是查询命令
            {
              SerialPortUtils.SendSearchCmd(data);
              //获取串口返回的数据
              byte[] returnData = GetReturnData(classroom);
              //将获取到的数据发送回去
              socket.Send(returnData);
              Console.WriteLine("服务端给客户端发送完成数据：" + socket.RemoteEndPoint);
            }
            else       //接收到的是执行命令
            {
              Console.WriteLine("接收到执行命令，发送完成");
              SerialPortUtils.SendCmd(data);
            }
          }
          break;
      }      
    }
    public byte[] GetReturnData(string classroom)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      //等待数据初始化
      while (!SerialPortUtils.DataDictionary.ContainsKey(classroom))
      {
        if (stopwatch.Elapsed.Seconds >= 2)//2秒后获取不到数据，则返回
        {
          Console.WriteLine("-------获取串口数据失败，返回1个长度数据包-----");
          return new byte[] { 0 };
        }
      }
      Console.WriteLine("=========获取到串口完整数据=======");
      Console.WriteLine($"字典key为：{classroom} 的数据被消费");
      byte[] data = SerialPortUtils.DataDictionary[classroom];
      SerialPortUtils.SendCmd(data);
      SerialPortUtils.DataDictionary.Remove(classroom);
      return data;
    }
  }
}
