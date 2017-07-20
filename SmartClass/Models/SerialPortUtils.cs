using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using Common.Exception;
using Model.Actuators;
using Model.Properties;
using System.Diagnostics;

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


        //private static byte[] data = new byte[1024];
        public static int Offset = 0;

        public static int ActuatorDataLength = 2;
        /// <summary>
        /// 无线串口号
        /// </summary>
        private static string COM = ConfigurationManager.AppSettings["COM"];

        public static Queue<byte[]> DataQueue = new Queue<byte[]>();
        public static Queue<ClassRoom> DataQueues = new Queue<ClassRoom>();
        //SerialPortService service = new SerialPortService();
        static SerialPortUtils()
        {
            Port = new SerialPort(COM);
            Port.BaudRate = 115200;
            Port.ReadBufferSize = 1024;
            Port.DataReceived += Port_DataReceived;
            Port.Open();
        }
        static List<byte> byteList = new List<byte>();
        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            byte[] data = new byte[1024];

            int len = Port.Read(data, Offset, data.Length - Offset);
            int length = 7 + data[6] + 3;   //数据包总长度
            Offset += len;
            while (Offset < length)     //当前读到的长度是否等于总长度
            {
                len = Port.Read(data, Offset, length - Offset);
                length = 7 + data[6] + 3;
                Offset += len;
            }
            if (data[Offset - 1] == 0xbb) //校验包尾
            {
                byte[] _data = new byte[Offset - 3];
                Array.Copy(data, 0, _data, 0, Offset - 3);
                byte[] _dataCrc = Common.CRC16.Crc(_data);
                if (_dataCrc[0] == data[Offset - 3] && _dataCrc[1] == data[Offset - 2]) //CRC的校验
                {
                    if (data[4] == 0x1f)
                    {
                        DataQueue.Enqueue(data);
                    }
                }
                else
                {
                    Offset = 0;
                }

            }
            Offset = 0;
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