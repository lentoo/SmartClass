using Common;

using SmartClass.Models.Enum;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
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

        private const string Online = "OnLine";
        private const string Offline = "OffLine";
        /// <summary>
        /// 传感器集合
        /// </summary>
        public static List<SonserBase> actuators { get; set; }

        public static int ActuatorDataLength = 2;
        /// <summary>
        /// 执行器
        /// </summary>
        private static string[] Sonsers = { "照明灯", "门", "窗帘", "窗户", "风机", "人体", "气体", "报警" };
        /// <summary>
        /// 传感器
        /// </summary>
        private static string[] Analogue = { "温度", "光照", "PM2.5", "空调", "电子钟", "投影屏" };
        /// <summary>
        /// 无线串口号
        /// </summary>
        private static string COM = ConfigurationManager.AppSettings["COM"];
        static SerialPortUtils()
        {
            Port = new SerialPort(COM);
            Port.BaudRate = 115200;
            Port.DataReceived += Port_DataReceived;
            Port.Open();
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            byte[] data = new byte[1024];
            int len = Port.Read(data, 0, data.Length);
            int index = 7;
            if (data[4] != 0x1f)
            {
                return;
            }
            if (data[0] == 0x55)
            {
                actuators = new List<SonserBase>();
                //处理数字量数据
                for (int i = 0; i < Sonsers.Length; i++)
                {
                    int SonserNum = data[index++];      //传感器数量
                    int SonserOnLineNum = data[index++];  //传感器在线数
                    ProcessActuatorData(SonserNum, ref index, data, Sonsers[i]);
                }
                //处理模拟量数据
                for (int i = 0; i < Analogue.Length; i++)
                {
                    int SonserNum = data[index++];   //模拟量数量数
                    int SonserOffLineNum = data[index++];  //模拟量在线数 
                    ProcessAnalogueData(SonserNum, ref index, data, Analogue[i]);
                }
            }
        }
        /// <summary>
        /// 执行器数据处理
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="name">传感器名称</param>
        private static void ProcessActuatorData(int num, ref int index, byte[] data, string name)
        {
            for (int i = 0; i < num; i++)
            {
                Actuator actuator = new Actuator();
                actuator.Id = Convert.ToString(data[index++], 16);
                actuator.Name = name;
                int state = data[index++];
                actuator.State = GetState(state);
                actuator.Online = state == 0 ? Offline : Online;
                actuators.Add(actuator);
            }
        }

        /// <summary>
        /// 模拟量数据数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="name">传感器名称</param>
        private static void ProcessAnalogueData(int num, ref int index, byte[] data, string name)
        {

            if (name == "温度")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digitalWD = new Digital();
                    digitalWD.Id = Convert.ToString(data[index++], 16);
                    double wd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalWD.value = wd + "℃";
                    digitalWD.Name = "温度";
                    digitalWD.Online = wd == 0 ? Offline : Online;
                    actuators.Add(digitalWD);
                    Digital digitalSD = new Digital();
                    digitalSD.Id = digitalWD.Id;
                    double sd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalSD.value = sd + "%";
                    digitalSD.Name = "湿度";
                    digitalSD.Online = sd == 0 ? Offline : Online;
                    actuators.Add(digitalSD);
                }
            }
            else if (name == "PM2.5")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    double value = Convert.ToDouble(data[index++] << 8 | data[index++]) / 100;
                    digital.value = value + "µg/m³";
                    digital.Name = "PM2.5";
                    digital.Online = value == 0 ? Offline : Online;
                    actuators.Add(digital);
                }
            }
            else if (name == "空调")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    Int16 iState = data[index++];
                    digital.state = ((iState >> 7) & 0x01) == 1 ? "开机" : "关机";
                    digital.value = (0x0f & data[index++]) + 16 + "℃";
                    digital.Name = name;
                    actuators.Add(digital);
                }
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    double value = data[index++];
                    digital.value = value+ "";
                    digital.Name = name;
                    digital.Online = value == 0 ? Offline : Online;
                    actuators.Add(digital);
                }
            }
        }
        /// <summary>
        /// 根据状态码返回状态
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string GetState(int i)
        {
            string state;
            switch (i)
            {
                case 0:
                    state = "不在线";
                    break;
                case 1:
                    state = "关闭";
                    break;
                case 2:
                    state = "打开";
                    break;
                case 6:
                    state = "停止";
                    break;
                default:
                    state = i + ",无此状态码";
                    break;
            }
            return state;
        }
        /// <summary>
        /// 向无线串口发送数据
        /// </summary>
        /// <param name="cmd">发送命令</param>
        public static bool SendCmd(byte[] data)
        {
            try
            {
                Port.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                ExceptionHelper.AddException(e);
            }
            return true;
        }
    }
}