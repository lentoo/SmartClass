using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using Common.Exception;
using Model.Actuators;
using Model.Properties;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

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
        //SerialPortService service = new SerialPortService();
        static SerialPortUtils()
        {
            Port = new SerialPort(COM);
            Port.BaudRate = 115200;
            Port.ReadBufferSize = 1024;
            //Port.ReadTimeout = 60000;
            Port.DataReceived += Port_DataReceived;
            Port.Open();
        }
        static List<byte> byteList = new List<byte>();
        // static byte[] data = new byte[1024];
        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int len = Port.BytesToRead;
                //int offset = 0;
                byte[] buf = new byte[len];
                Port.Read(buf, 0, len);
                byteList.AddRange(buf);
                while (byteList.Count >= 10)
                {
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
                                byte[] _dataCrc = Common.CRC16.Crc(_data);
                                if (_dataCrc[0] == byteList[length - 3] && _dataCrc[1] == byteList[length - 2]) //CRC的校验
                                {
                                    buf = new byte[length];
                                    byteList.CopyTo(0, buf, 0, length);
                                    byteList.RemoveRange(0, length);
                                    DataQueue.Enqueue(buf);
                                }
                            }else
                            {
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                                byteList.RemoveAt(0);
                            }
                        }else
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

            #region 注释代码


            //int Offset = 0;
            //byte[] data = new byte[1024];

            //int size = Port.BytesToRead;
            //int len = Port.Read(data, Offset, data.Length - Offset);
            //int length = 7 + data[6] + 3;   //数据包总长度
            //Offset += len;
            //while (Offset < length)     //当前读到的长度是否等于总长度
            //{
            //    len = Port.Read(data, Offset, length - Offset);
            //    length = 7 + data[6] + 3;
            //    Offset += len;
            //}
            //if (data[4] != 0x1f) return;
            //if (data[Offset - 1] == 0xbb) //校验包尾
            //{
            //    byte[] _data = new byte[Offset - 3];
            //    Array.Copy(data, 0, _data, 0, Offset - 3);
            //    byte[] _dataCrc = Common.CRC16.Crc(_data);
            //    if (_dataCrc[0] == data[Offset - 3] && _dataCrc[1] == data[Offset - 2]) //CRC的校验
            //    {
            //        DataQueue.Enqueue(data);
            //    }
            //    else
            //    {
            //        Offset = 0;
            //    }
            //}
            //Offset = 0;
            //Port.DiscardInBuffer();
            #endregion
        }

        private static byte[] Cmd = null;
        /// <summary>
        /// 向无线串口发送数据
        /// </summary>
        /// <param name="cmd">发送命令</param>
        public static bool SendCmd(byte[] cmd)
        {
            try
            {
                Cmd = cmd;
                // Port.DiscardInBuffer();
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