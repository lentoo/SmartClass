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

namespace SmartClass.Models
{
  /// <summary>
  /// 串口工具类
  /// </summary>
  public class SerialPortUtils
  {
    public static void InitialSerialPort()
    {
      SerialPort port = Port;
    }
    /// <summary>
    /// 无线串口
    /// </summary>
    private static SerialPort Port
    {
      get
      {
        if (Port == null)
        {
          Port = new SerialPort(COM);
          Port.BaudRate = 115200;
          Port.ReadBufferSize = 1024;
          Port.DataBits = 8;
          Port.StopBits = StopBits.One;
          //Port.ReadTimeout = 60000;
          Port.DataReceived += Port_DataReceived;
          Port.Open();
        }
        return Port;
      }
      set { Port = value; }
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

    //static SerialPortUtils()
    //{
    //  Port = new SerialPort(COM);
    //  Port.BaudRate = 115200;
    //  Port.ReadBufferSize = 1024;
    //  Port.DataBits = 8;
    //  Port.StopBits = StopBits.One;
    //  //Port.ReadTimeout = 60000;
    //  Port.DataReceived += Port_DataReceived;
    //  Port.Open();
    //}

    public static void ClosePort()
    {
      if (Port.IsOpen)
      {
        Port.Close();
        Port.Dispose();
      }
    }

    private static readonly List<byte> ByteList = new List<byte>();
    private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      try
      {
        int len = Port.BytesToRead;
        byte[] buf = new byte[len];
        Port.Read(buf, 0, len);
        ByteList.AddRange(buf);
        Debug.WriteLine("读到的数据长度" + len);
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
                  Debug.WriteLine(classroom);
                  DataDictionary.Add(classroom, buf);
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
        ExceptionHelper.AddException(exception);
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
          Task.Delay(TimeSpan.FromMilliseconds(150));
          //Thread.Sleep(TimeSpan.FromMilliseconds(150));
        }
      }
      catch (Exception e)
      {
        ExceptionHelper.AddException(e);
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