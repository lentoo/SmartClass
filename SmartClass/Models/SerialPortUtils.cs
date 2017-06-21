using Common;
using SmartClass.Models.Actuators;
using SmartClass.Models.Enum;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;

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
        private static SerialPort Port { get; set; }
        /// <summary>
        /// 传感器集合
        /// </summary>
        public static List<SonserBase> actuators { get; set; }
        
        /// <summary>
        /// 执行器
        /// </summary>
        private static string[] Sonsers = { "照明", "门", "窗帘", "窗户", "风机", "人体", "气体", "报警" };
        /// <summary>
        /// 传感器
        /// </summary>
        private static string[] Analogue = { "温度", "光照", "PM2.5", "空调", "电子钟" };
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
                    int SonserNum = data[index++];   //传感器在线数
                    int SonserOpenNum = data[index++];  //传感器打开数
                    ProcessActuatorData(SonserNum, ref index, data, Sonsers[i]);
                }
                //处理模拟量数据
                for (int i = 0; i < Analogue.Length; i++)
                {
                    int SonserNum = data[index++];   //传感器在线数
                    int SonserOpenNum = data[index++];  //传感器打开数 
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
                actuator.Id = Convert.ToInt32(Convert.ToString(data[index++], 10));
                actuator.Name = name;
                actuator.State = GetState(data[index++]);
                actuators.Add(actuator);
            }
        }
        /// <summary>
        /// 根据状态码返回状态
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static string GetState(int i)
        {
            string state = "";
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
                case 3:
                    state = "开25%";
                    break;
                case 4:
                    state = "开50%";
                    break;
                case 5:
                    state = "开75%";
                    break;
                case 6:
                    state = "既不是开也不是关";
                    break;
            }
            return state;
        }

        private static void ProcessAnalogueData(int num, ref int index, byte[] data, string name)
        {

            if (name == "温度")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digitalWD = new Digital();
                    digitalWD.Id =data[index++];
                    digitalWD.value = (data[index++] << 8 | data[index++]) / 10 + "℃";
                    digitalWD.Name = "温度";
                    actuators.Add(digitalWD);
                    Digital digitalSD = new Digital();
                    digitalSD.Id = digitalWD.Id;
                    digitalSD.value = (data[index++] << 8 | data[index++]) / 10 + "%";
                    digitalSD.Name = "湿度";
                    actuators.Add(digitalSD);
                }
            }
            else if (name == "PM2.5")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digitalWD = new Digital();
                    digitalWD.Id = data[index++];
                    digitalWD.value = (data[index++] << 8 | data[index++]) + "";
                    digitalWD.Name = "PM2.5";
                    actuators.Add(digitalWD);
                }
            }
            else if (name == "空调")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = data[index++];
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
                    Digital digitalWD = new Digital();
                    digitalWD.Id = data[index++];
                    digitalWD.value = data[index++] + "";
                    digitalWD.Name = name;
                    actuators.Add(digitalWD);
                }
            }
        }

        /// <summary>
        /// 向无线串口发送数据
        /// </summary>
        /// <param name="cmd"></param>
        public static bool SendCmd(byte[] data)
        {
            Port.Write(data, 0, data.Length);            
            return true;
        }
    }
}