using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using SmartClass.Infrastructure.Exception;
using Model.Actuators;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using SmartClass.Infrastructure.Extended;

namespace SerialPortServer
{
  /// <summary>
  /// 串口工具类
  /// </summary>
  public class SerialPortUtils
  {
    private static SerialPort port = null;
    public static SerialPort InitialSerialPort()
    {
      if (port == null)
      {
        port = new SerialPort(COM);
        port.BaudRate = 115200;
        port.ReadBufferSize = 1024;
        port.DataBits = 8;
        port.StopBits = StopBits.One;
        port.ReceivedBytesThreshold = 1;
        //Port.ReadTimeout = 60000;
        port.DataReceived += Port_DataReceived;
        port.Open();
      }
      return port;
    }
    public static CountdownEvent countdown = new CountdownEvent(1);    
    /// <summary>
    /// 无线串口
    /// </summary>
    private static SerialPort Port
    {
      get
      {
        var _port = InitialSerialPort();
        if (!_port.IsOpen)
        {
          _port.Open();
        }
        return _port;
      }
    }

    //static int Offset = 0;

    /// <summary>
    /// 无线串口号
    /// </summary>
    private static string COM = ConfigurationManager.AppSettings["COM"];

    /// <summary>
    /// 查询数据队列
    /// </summary>
    public static Dictionary<string, byte[]> DataDictionary = new Dictionary<string, byte[]>();
    /// <summary>
    /// 接收到报警数据队列
    /// </summary>
    public static Queue<byte[]> AlarmData = new Queue<byte[]>();

    public static void ClosePort()
    {
      if ((port?.IsOpen)==true)
      {

        port.Close();
        port.Dispose();
      }
    }

    private static readonly List<byte> ByteList = new List<byte>();

    private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        Console.WriteLine("开始接收串口发来的数据");
        int len = Port.BytesToRead;
        byte[] buf = new byte[len];
        Port.Read(buf, 0, len);
        ByteList.AddRange(buf);
        Console.WriteLine("读到的数据长度" + len);
        #region 测试专用
        if (len == 1)
        {
          if (ByteList[0] == 0xE0)
          {
            SendCmd(new byte[] { 0x55, 0x02, 0x12, 0x34, 0x1f, 0x00, 0x01, 0x00, 0x3a, 0x12, 0xbb });

          }
          ByteList.RemoveAt(0);
        }
        #endregion
        #region 对串口数据进行处理
        while (ByteList.Count >= 10)
        {
          //查找数据标头
          if (ByteList[0] == 0x55)
          {
            if (ByteList[1] == 0x02)
            {
              if (ByteList[4] == 0x1f)
              {
                int length = ByteList[6] + 10;//数据包长度
                if (length == 11) //接收到是的查询命令，不是返回的数据
                {
                  ByteList.RemoveRange(0, ByteList.Count);
                  break;
                }
                if (ByteList.Count < length)  //数据未接收完毕，跳出循环
                {
                  break;
                }
                byte[] _data = new byte[length - 3];
                Array.Copy(ByteList.ToArray(), 0, _data, 0, length - 3);
                byte[] _dataCrc = _data.Crc();
                if (_dataCrc[0] == ByteList[length - 3] && _dataCrc[1] == ByteList[length - 2]) //CRC的校验
                {
                  buf = new byte[length];
                  ByteList.CopyTo(0, buf, 0, length);
                  ByteList.RemoveRange(0, length);
                  //教室地址
                  string classroom = Convert.ToString(buf[2], 16) + Convert.ToString(buf[3], 16);
                  if (DataDictionary.ContainsKey(classroom))
                  {
                    Console.WriteLine("有一条未消费的数据，key值为：" + classroom);
                    DataDictionary.Remove(classroom);
                  }
                  Console.WriteLine("一条数据已经添加到字典中，key值为：" + classroom);
                  DataDictionary.Add(classroom, buf);                  
                  countdown.Signal();
                }
              }
              else if (ByteList[4] == 0x07)   //表示接收到教室控制器发送过来的报警数据
              {
                int length = ByteList[6] + 10;
                if (ByteList.Count < length)  //数据未接收完毕，跳出循环
                {
                  break;
                }
                byte[] _data = new byte[length - 3];
                Array.Copy(ByteList.ToArray(), 0, _data, 0, length - 3);
                byte[] _dataCrc = _data.Crc();
                if (_dataCrc[0] == ByteList[length - 3] && _dataCrc[1] == ByteList[length - 2]) //CRC的校验
                {
                  buf = new byte[length];
                  ByteList.CopyTo(0, buf, 0, length);
                  ByteList.RemoveRange(0, length);

                  AlarmData.Enqueue(buf);
                }
              }
              else //目前不需要的数据
              {
                ByteList.RemoveAt(0);
                ByteList.RemoveAt(0);
                ByteList.RemoveAt(0);
                ByteList.RemoveAt(0);
              }
            }
            else
            {
              ByteList.RemoveAt(0);
              ByteList.RemoveAt(0);
            }
          }
          else
          {
            ByteList.RemoveAt(0);
          }
        }
        #endregion
      }
      catch (Exception exception)
      {
        throw exception;
      }
    }
    /// <summary>
    /// 记录发送的指令
    /// </summary>
    private static byte[] Cmd;
    /// <summary>
    /// 向串口写数据的锁
    /// </summary>
    private static readonly object GetWriteLock = new object();
    /// <summary>
    /// 向无线串口发送查询数据
    /// </summary>
    /// <param name="cmd">发送命令</param>
    public static bool SendSearchCmd(byte[] cmd)
    {
      try
      {
        //TODO 每次向串口写查询命令时，必须间隔150ms，视情况而定，
        lock (GetWriteLock)
        {
          Cmd = cmd;
          Port.Write(cmd, 0, cmd.Length);
          Console.WriteLine("发送完成");
          //Task.Delay(TimeSpan.FromMilliseconds(150));          
        }
      }
      catch (Exception e)
      {
        throw e;
      }
      return true;
    }
    /// <summary>
    /// 向无线串口发送数据
    /// </summary>
    /// <param name="cmd">发送命令</param>
    public static bool SendCmd(byte[] cmd)
    {
      try
      {
        Cmd = cmd;
        Port.Write(cmd, 0, cmd.Length);
      }
      catch (Exception e)
      {
        ExceptionHelper.AddException(e);
      }
      return true;
    }
  }
}