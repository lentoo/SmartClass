using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Model;
using Model.Properties;
using SmartClass.Models.Types;

namespace SmartClass.Models
{
    public class SerialPortService
    {
        /// <summary>
        /// 数字量传感器种类
        /// </summary>
        private string[] Sonsers = ConfigurationManager.AppSettings["Sonsers"].Split(',');

        /// <summary>
        /// 模拟量传感器种类
        /// </summary>
        private string[] Analogue = ConfigurationManager.AppSettings["Analogue"].Split(',');
        private List<SonserBase> actuators { get; set; }
        private static object lockObj = new object();
        private byte[] data { get; set; }
        /// <summary>
        /// 向串口发送数据
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCmd(byte[] cmd)
        {
            SerialPortUtils.SendCmd(cmd);
        }

        /// <summary>
        /// 获取串口返回的数据
        /// </summary>
        /// <returns></returns>
        public ClassRoom GetReturnData()
        {
            //等待数据初始化
            //while (SerialPortUtils.DataQueue.Count <= 0)
            //{
            //    Thread.Sleep(50);
            //}
            if (SerialPortUtils.DataQueue.Count <= 0)
            {
                Thread.Sleep(100);
            }
            if (SerialPortUtils.DataQueue.Count <= 0)
            {
                return null;
            }
            byte[] _data = SerialPortUtils.DataQueue.Dequeue();
            this.data = _data;
            ClassRoom classRoom = Init();
            return classRoom;
        }

        /// <summary>
        /// 对串口数据进行处理
        /// </summary>
        /// <returns></returns>
        private ClassRoom Init()
        {
            ClassRoom classRoom = new ClassRoom();
            int index = 7;
            if (data[4] != 0x1f)
            {
                return null;
            }
            if (data[0] == 0x55)
            {
                string classRoomId = Convert.ToString(data[2], 16) + Convert.ToString(data[3], 16);
                classRoom.Id = classRoomId;
                actuators = new List<SonserBase>();
                //处理数字量数据
                for (int i = 0; i < Sonsers.Length; i++)
                {
                    int SonserNum = data[index++];      //传感器数量
                    int SonserOnLineNum = data[index++];  //传感器在线数
                    ProcessActuatorData(SonserNum, ref index, data, i);
                }
                //处理模拟量数据
                for (int i = 0; i < Analogue.Length; i++)
                {
                    int SonserNum = data[index++];   //模拟量数量数
                    int SonserOffLineNum = data[index++];  //模拟量在线数 
                    ProcessAnalogueData(SonserNum, ref index, data, i);
                }
                classRoom.SonserList = actuators;
            }
            return classRoom;
        }
        /// <summary>
        /// 执行器数据处理
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="name">设备类型</param>
        private void ProcessActuatorData(int num, ref int index, byte[] data, int type)
        {
            string name = Sonsers[type];
            type += 1;
            for (int i = 0; i < num; i++)
            {
                Actuator actuator = new Actuator();
                actuator.Id = Convert.ToString(data[index++], 16);
                actuator.Name = name;

                actuator.Type = type;
                int state = data[index++];
                actuator.State = GetState(state);
                actuator.IsOpen = state == 1 ? false : true;
                actuator.Online = state == 0 ? StateType.Offline : StateType.Online;
                actuators.Add(actuator);
            }
        }
        /// <summary>
        /// 模拟量数据数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="name">设备类型</param>
        private void ProcessAnalogueData(int num, ref int index, byte[] data, int type)
        {
            string name = Analogue[type];
            type += Sonsers.Length + 1;
            if (name == "温度")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digitalWD = new Digital();
                    digitalWD.Id = Convert.ToString(data[index++], 16);
                    double wd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalWD.value = wd + "℃";
                    digitalWD.Name = "温度";
                    digitalWD.Type = type;
                    digitalWD.Online = wd == 0 ? StateType.Offline : StateType.Online;

                    actuators.Add(digitalWD);
                    Digital digitalSD = new Digital();
                    digitalSD.Id = digitalWD.Id;
                    double sd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalSD.value = sd + "%";
                    digitalSD.Name = "湿度";
                    digitalSD.Type = type;
                    digitalSD.Online = sd == 0 ? StateType.Offline : StateType.Online;
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
                    digital.Type = type;
                    digital.Online = value == 0 ? StateType.Offline : StateType.Online;
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
                    digital.State = ((iState >> 7) & 0x01) == 1 ? "打开" : "关闭";
                    digital.value = (0x0f & data[index++]) + 16 + "℃";
                    digital.Name = name;
                    digital.Type = type;
                    actuators.Add(digital);
                }
            }
            else if (name == "光照")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    double value = data[index++] << 8 | data[index++];
                    digital.value = value + "lx";
                    digital.Name = name;
                    digital.Type = type;
                    digital.Online = value == 0 ? StateType.Offline : StateType.Online;
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
                    digital.value = value + "";
                    digital.Name = name;
                    digital.Type = type;
                    digital.Online = value == 0 ? StateType.Offline : StateType.Online;
                    actuators.Add(digital);
                }
            }
        }
        /// <summary>
        /// 根据状态码返回状态
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private string GetState(int i)
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
    }
}