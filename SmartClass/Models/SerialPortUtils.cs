using Common;

using SmartClass.Models.Enum;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.Ajax.Utilities;
using Model;

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
        public static bool Possession;
        public static int offset = 0;

        public static int ActuatorDataLength = 2;
        /// <summary>
        /// 无线串口号
        /// </summary>
        private static string COM = ConfigurationManager.AppSettings["COM"];

        public static Queue<byte[]> DataQueue = new Queue<byte[]>();
        static SerialPortUtils()
        {
            Port = new SerialPort(COM);
            Port.BaudRate = 115200;
            Port.DataReceived += Port_DataReceived;
            Port.Open();
        }
        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(50);
            int len = Port.Read(data, offset, data.Length - offset);
            if (data[4] != 0x1f)
            {
                return;
            }
            int length = 7 + data[6] + 3;   //数据包总长度
            offset += len;
            while (offset < length)     //当前读到的长度是否等于总长度
            {
                len = Port.Read(data, offset, length - offset);
                offset += len;
            }
            if (data[offset - 1] == 0xbb) //校验包尾
            {
                byte[] _data = new byte[offset - 3];
                Array.Copy(data, 0, _data, 0, offset - 3);
                byte[] _dataCrc = Common.CRC16.Crc(_data);
                if (_dataCrc[0] == data[offset - 3] && _dataCrc[1] == data[offset - 2]) //CRC的校验
                {
                    DataQueue.Enqueue(data);
                }
                else
                {
                    offset = 0;
                }
                offset = 0;
            }
            else
            {
                SendCmd(Cmd);
                offset = 0;
            }
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
                Possession = false;
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