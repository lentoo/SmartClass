using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Model;
using Model.Actuators;
using SmartClass.Models.Types;
using System.IO.Ports;
using Models.Classes;
using Model.Result;
using Common;
using IBLL;
using Model.Enum;
using SmartClass.Models.Exceptions;

namespace SmartClass.Models
{
    public class SerialPortService
    {
        public IZ_EquipmentService ZEquipmentService { get; set; }
        /// <summary>
        /// 数字量传感器种类
        /// </summary>
        private string[] Sonsers = ConfigurationManager.AppSettings["Sonsers"].Split(',');

        /// <summary>
        /// 模拟量传感器种类
        /// </summary>
        private string[] Analogue = ConfigurationManager.AppSettings["Analogue"].Split(',');
        private List<SonserBase> Actuators { get; set; }

        //public delegate void Port_DataReceived(object sender, SerialDataReceivedEventArgs e);

        //public event Port_DataReceived PortDataReceived;

        private byte[] Data { get; set; }
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //等待数据初始化
            while (SerialPortUtils.DataQueue.Count <= 0)
            {
                if (stopwatch.Elapsed.Seconds >= 3)//3秒后获取不到数据，则返回
                {
                    return null;
                }
            }
            Data = SerialPortUtils.DataQueue.Dequeue();
            ClassRoom classRoom = null;
            classRoom = Init(classRoom);
            // ClassRoom classroom = SerialPortUtils.DataQueues.Dequeue();
            return classRoom;
        }

        public ClassRoom GetReturnDataTest()
        {   //TODO 测试数据
            while (SerialPortUtils.DataQueue.Count <= 0)
            {
                ;
            }
            Data = SerialPortUtils.DataQueue.Dequeue();
            ClassRoom classRoom = null;
            classRoom = Init(classRoom);
            // ClassRoom classroom = SerialPortUtils.DataQueues.Dequeue();
            return classRoom;
        }

        //private ClassRoom classRoom;
        /// <summary>
        /// 对串口数据进行处理
        /// </summary>
        /// <returns></returns>
        public ClassRoom Init(ClassRoom classRoom)
        {
            classRoom = classRoom ?? new ClassRoom();
            int index = 7;
            if (Data[4] != 0x1f)
            {
                return null;
            }
            if (Data[0] == 0x55)
            {
                string classRoomId = Convert.ToString(Data[2], 16) + Convert.ToString(Data[3], 16);
                classRoom.Id = classRoomId;
                Actuators = new List<SonserBase>();
                //处理数字量数据
                for (int i = 0; i < Sonsers.Length; i++)
                {
                    int sonserNum = Data[index++];      //传感器数量
                    int sonserOnLineNum = Data[index++];  //传感器在线数
                    ProcessActuatorData(sonserNum, ref index, Data, i);
                }
                //处理模拟量数据
                for (int i = 0; i < Analogue.Length; i++)
                {
                    int sonserNum = Data[index++];   //模拟量数量数
                    int sonserOffLineNum = Data[index++];  //模拟量在线数 
                    ProcessAnalogueData(sonserNum, ref index, Data, i);
                }
                classRoom.SonserList = Actuators;
            }
            return classRoom;
        }

        /// <summary>
        /// 执行器数据处理
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="type">类型编码</param>
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
                actuator.Controllable = name == "人体" ? false : name == "气体" ? false : true;
                Actuators.Add(actuator);
            }
        }
        /// <summary>
        /// 模拟量数据数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="type">类型编码</param>
        private void ProcessAnalogueData(int num, ref int index, byte[] data, int type)
        {
            string name = Analogue[type];
            type += Sonsers.Length + 1;
            if (name == "温度")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digitalWd = new Digital();
                    digitalWd.Id = Convert.ToString(data[index++], 16);
                    double wd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalWd.value = wd + "℃";
                    digitalWd.Name = "温度";
                    digitalWd.Type = type;
                    digitalWd.Online = wd == 0 ? StateType.Offline : StateType.Online;
                    digitalWd.State = wd == 0 ? StateType.StateClose : StateType.StateOpen;
                    digitalWd.Controllable = false;
                    Actuators.Add(digitalWd);
                    Digital digitalSd = new Digital();
                    digitalSd.Id = digitalWd.Id;
                    double sd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                    digitalSd.value = sd + "%";
                    digitalSd.Name = "湿度";
                    digitalSd.Type = type;
                    digitalSd.Online = sd == 0 ? StateType.Offline : StateType.Online;
                    digitalSd.State = sd == 0 ? StateType.StateClose : StateType.StateOpen;
                    digitalSd.Controllable = false;
                    Actuators.Add(digitalSd);
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
                    digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                    digital.Controllable = false;
                    Actuators.Add(digital);
                }
            }
            else if (name == "空调")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    Int16 iState = data[index++];
                    digital.State = ((iState >> 7) & 0x01) == 1 ? StateType.StateOpen : StateType.StateClose;
                    float val = (0x0f & data[index++]);
                    digital.value = val + 16 + "℃";
                    digital.Online = val == 0 ? StateType.Offline : StateType.Online;
                    digital.Name = name;
                    digital.Controllable = true;
                    digital.Type = type;
                    Actuators.Add(digital);
                }
            }
            else if (name == "光照")
            {
                for (int i = 0; i < num; i++)
                {
                    Digital digital = new Digital();
                    digital.Id = Convert.ToString(data[index++], 16);
                    double value = data[index++] << 8 | data[index++];
                    digital.value = value + " lx";
                    digital.Name = name;
                    digital.Type = type;
                    digital.Online = value == 0 ? StateType.Offline : StateType.Online;
                    digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                    digital.Controllable = false;
                    Actuators.Add(digital);
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
                    digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                    digital.Controllable = false;
                    Actuators.Add(digital);
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

        /// <summary>
        /// 发送转换后的命令
        /// </summary>
        /// <param name="fun">功能码</param>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public EquipmentResult SendConvertCmd(byte fun, string classroom, string nodeAdd, byte onoff)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "00" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                if (nodeAdd != "00")    //节点地址00表示查询所有设备信息
                {
                    if (!ZEquipmentService.CheckClassEquipment(classroom, nodeAdd))
                    {
                        throw new EquipmentNoFindException("没有查询到该教室有该ID的设备");
                    }
                }
                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, onoff };
                byte[] bclassroom = CmdUtils.StrToHexByte(classroom);
                byte[] bnodeAdd = CmdUtils.StrToHexByte(nodeAdd);
                bclassroom.CopyTo(cmd, 2);
                bnodeAdd.CopyTo(cmd, 5);
                cmd = CmdUtils.ActuatorCommand(cmd);

                SendCmd(cmd);
                oa.Status = true;
                oa.ResultCode = ResultCode.Ok;
            }
            catch (Exception exception)
            {
                oa.Status = false;
                oa.ErrorData = exception.Message;
                oa.ResultCode = ResultCode.Error;
                //ExceptionHelper.AddException(exception);
            }
            return oa;
        }
        /// <summary>
        /// 发送转换后的命令
        /// </summary>
        /// <param name="fun">功能码</param>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="height">高位</param>
        /// <param name="low">低位</param>
        /// <returns></returns>
        public EquipmentResult SendConvertCmd(byte fun, string classroom, string nodeAdd, byte height, byte low)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "00" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                if (nodeAdd != "00")
                {
                    if (!ZEquipmentService.CheckClassEquipment(classroom, nodeAdd))
                    {
                        throw new EquipmentNoFindException("没有查询到该教室有该ID的设备");
                    }
                }

                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x02, height, low };
                byte[] bclassroom = CmdUtils.StrToHexByte(classroom);
                byte[] bnodeAdd = CmdUtils.StrToHexByte(nodeAdd);
                bclassroom.CopyTo(cmd, 2);
                bnodeAdd.CopyTo(cmd, 5);
                cmd = CmdUtils.ActuatorCommand(cmd);

                SendCmd(cmd);
                //SerialPortUtils.SendCmd(cmd);
                oa.Status = true;
                oa.ResultCode = ResultCode.Ok;
            }
            catch (Exception exception)
            {
                oa.Status = false;
                oa.ErrorData = exception.Message;
                oa.ResultCode = ResultCode.Error;
                //ExceptionHelper.AddException(exception);
            }
            return oa;
        }
    }
}