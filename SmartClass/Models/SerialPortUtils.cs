﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using Common.Exception;

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
        public static int Offset = 0;

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
            int len = Port.Read(data, Offset, data.Length - Offset);
            if (data[4] != 0x1f)
            {
                return;
            }
            int length = 7 + data[6] + 3;   //数据包总长度
            Offset += len;
            while (Offset < length)     //当前读到的长度是否等于总长度
            {
                len = Port.Read(data, Offset, length - Offset);
                Offset += len;
            }
            if (data[Offset - 1] == 0xbb) //校验包尾
            {
                byte[] _data = new byte[Offset - 3];
                Array.Copy(data, 0, _data, 0, Offset - 3);
                byte[] _dataCrc = Common.CRC16.Crc(_data);
                if (_dataCrc[0] == data[Offset - 3] && _dataCrc[1] == data[Offset - 2]) //CRC的校验
                {
                    DataQueue.Enqueue(data);
                }
                Offset = 0;
            }
            else
            {
                SendCmd(Cmd);
                Offset = 0;
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