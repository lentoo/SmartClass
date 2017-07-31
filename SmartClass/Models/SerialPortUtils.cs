using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using Common.Exception;
using Model.Actuators;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Common.Extended;

namespace SmartClass.Models
{
    /// <summary>
    /// 串口工具类
    /// </summary>
    public class SerialPortUtils
    {
        /// <summary>
        /// 无线串口
        /// </summary>
        private static SerialPort Port { get; }
        
        private static byte[] data = new byte[1024];
        //static int Offset = 0;

        /// <summary>
        /// 无线串口号
        /// </summary>
        private static string COM = ConfigurationManager.AppSettings["COM"];

        public static Queue<byte[]> DataQueue = new Queue<byte[]>();
        static SerialPortUtils()
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
        static List<byte> byteList = new List<byte>();
        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = Port.BytesToRead;
                byte[] buf = new byte[len];
                Port.Read(buf, 0, len);
                byteList.AddRange(buf);
                Debug.WriteLine("读到的数据长度" + len);
                while (byteList.Count >= 10)
                {
                    Debug.WriteLine(1);
                    //查找数据标头
                    if (byteList[0] == 0x55)
                    {
                        if (byteList[1] == 0x02)
                        {
                            if (byteList[4] == 0x1f)
                            {
                                int length = byteList[6] + 10;//数据包长度
                                if (byteList.Count < length)  //数据未接收完毕，跳出循环
                                {
                                    break;
                                }
                                byte[] _data = new byte[length - 3];
                                Array.Copy(byteList.ToArray(), 0, _data, 0, length - 3);
                                byte[] _dataCrc =_data.Crc();
                                if (_dataCrc[0] == byteList[length - 3] && _dataCrc[1] == byteList[length - 2]) //CRC的校验
                                {
                                    buf = new byte[length];
                                    byteList.CopyTo(0, buf, 0, length);
                                    byteList.RemoveRange(0, length);

                                    DataQueue.Enqueue(buf);
                                }
                            }
                            else
                            {
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                            }
                        }
                        else
                        {
                            byteList.RemoveAt(0);
                            byteList.RemoveAt(0);
                        }
                    }
                    else
                    {
                        byteList.RemoveAt(0);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionHelper.AddException(exception);
            }
        }

        private static byte[] Cmd = null;
        private static object lockObject = new object();
        /// <summary>
        /// 向无线串口发送查询数据
        /// </summary>
        /// <param name="cmd">发送命令</param>
        public static bool SendSearchCmd(byte[] cmd)
        {
            try
            {
                lock (lockObject)
                {
                    Cmd = cmd;
                    Port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep(300);
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