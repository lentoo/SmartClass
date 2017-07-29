using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Model;
using SmartClass.Models.Types;
using System.IO.Ports;
using Models.Classes;
using Model.Result;
using Common;
using IBLL;
using Model.Enum;
using SmartClass.Models.Exceptions;
using Model.Actuators;
using Common.Exception;

namespace SmartClass.Models
{
    public class SerialPortService
    {
        public IZ_EquipmentService ZEquipmentService { get; set; }
        /// <summary>
        /// 数字量传感器种类
        /// </summary>
        private string[] Digital = ConfigurationManager.AppSettings["Digital"].Split(',');

        /// <summary>
        /// 模拟量传感器种类
        /// </summary>
        private string[] Analogue = ConfigurationManager.AppSettings["Analogue"].Split(',');

        private List<SensorBase> Sensors { get; set; }

        private byte[] Data { get; set; }
        /// <summary>
        /// 向串口发送执行数据
        /// </summary>
        /// <param name="cmd"></param>
        public void SendCmd(byte[] cmd)
        {
            SerialPortUtils.SendCmd(cmd);

        }
        /// <summary>
        /// 向串口发送查询数据
        /// </summary>
        /// <param name="cmd"></param>
        public void SendSearchCmd(byte[] cmd)
        {
            SerialPortUtils.SendSearchCmd(cmd);
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
                Sensors = new List<SensorBase>();
                //处理数字量数据
                for (int i = 0; i < Digital.Length; i++)
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
                classRoom.SonserList = Sensors;
            }
            return classRoom;
        }

        /// <summary>
        /// 数字量数据处理
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">下标</param>
        /// <param name="data">数据</param>
        /// <param name="type">类型编码</param>
        private void ProcessActuatorData(int num, ref int index, byte[] data, int type)
        {
            string name = Digital[type];
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
                Sensors.Add(actuator);
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
            type += Digital.Length + 1;
            if (name == "温度")
            {
                index = ProcessTemperatureAndHumidity(num, index, data, type);
            }
            else if (name == "PM2.5")
            {
                index = ProcessPM2_5(num, index, data, type);
            }
            else if (name == "空调")
            {
                index = ProcessAir(num, index, data, type);
            }
            else if (name == "光照")
            {
                index = ProcessIllumination(num, index, data, type);
            }
            else if (name == "投影屏")
            {
                index = ProcessProjectionScreen(num, index, data, type);
            }
            else
            {
                index = ProcessOther(num, index, data, type, name);
            }
        }

        /// <summary>
        /// 处理投影屏数据
        /// </summary>
        /// <param name="num"></param>
        /// <param name="index"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private int ProcessProjectionScreen(int num, int index, byte[] data, int type)
        {
            for (int i = 0; i < num; i++)
            {
                Actuator ProjectionScreen = new Actuator();
                ProjectionScreen.Id = Convert.ToString(data[index++], 16);
                int state = data[index++];
                ProjectionScreen.State = GetState(state);
                ProjectionScreen.IsOpen = state == 1 ? false : true;
                ProjectionScreen.Controllable = true;
                ProjectionScreen.Name = "投影屏";
                ProjectionScreen.Type = type;
                ProjectionScreen.Online = state == 0 ? StateType.Offline : StateType.Online;
                ProjectionScreen.State = state != 0 ? StateType.StateOpen : StateType.StateClose;
                ProjectionScreen.Controllable = false;
                Sensors.Add(ProjectionScreen);
            }

            return index;
        }

        /// <summary>
        /// 处理温湿度数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">索引</param>
        /// <param name="data">数据</param>
        /// <param name="type">传感器类型</param>
        /// <returns></returns>
        private int ProcessTemperatureAndHumidity(int num, int index, byte[] data, int type)
        {
            for (int i = 0; i < num; i++)
            {
                Digital digitalWd = new Digital();
                digitalWd.Id = Convert.ToString(data[index++], 16);
                double wd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                digitalWd.Value = wd + "℃";
                digitalWd.Name = "温度";
                digitalWd.Type = type;
                digitalWd.Online = wd == 0 ? StateType.Offline : StateType.Online;
                digitalWd.State = wd == 0 ? StateType.StateClose : StateType.StateOpen;
                digitalWd.Controllable = false;
                Sensors.Add(digitalWd);
                Digital digitalSd = new Digital();
                digitalSd.Id = digitalWd.Id;
                double sd = Convert.ToDouble(data[index++] << 8 | data[index++]) / 10;
                digitalSd.Value = sd + "%";
                digitalSd.Name = "湿度";
                digitalSd.Type = type;
                digitalSd.Online = sd == 0 ? StateType.Offline : StateType.Online;
                digitalSd.State = sd == 0 ? StateType.StateClose : StateType.StateOpen;
                digitalSd.Controllable = false;
                Sensors.Add(digitalSd);
            }

            return index;
        }

        /// <summary>
        /// 处理PM2.5的数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">索引</param>
        /// <param name="data">数据</param>
        /// <param name="type">传感器类型</param>
        /// <returns></returns>
        private int ProcessPM2_5(int num, int index, byte[] data, int type)
        {
            for (int i = 0; i < num; i++)
            {
                Digital digital = new Digital();
                digital.Id = Convert.ToString(data[index++], 16);
                double value = Convert.ToDouble(data[index++] << 8 | data[index++]) / 100;
                digital.Value = value + "µg/m³";
                digital.Name = "PM2.5";
                digital.Type = type;
                digital.Online = value == 0 ? StateType.Offline : StateType.Online;
                digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                digital.Controllable = false;
                Sensors.Add(digital);
            }

            return index;
        }
        /// <summary>
        /// 处理空调数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">索引</param>
        /// <param name="data">数据</param>
        /// <param name="type">传感器类型</param>
        /// <returns></returns>
        private int ProcessAir(int num, int index, byte[] data, int type)
        {
            for (int i = 0; i < num; i++)
            {
                AirConditioning airConditioning = new AirConditioning();
                airConditioning.Id = Convert.ToString(data[index++], 16);
                byte height = data[index++];
                airConditioning.State = ((height >> 7) & 0x01) == 1 ? StateType.StateOpen : StateType.StateClose;
                airConditioning.IsOpen = airConditioning.State == StateType.StateOpen;
                byte low = data[index++];
                if (height == 0 && low == 0) airConditioning.Online = StateType.Offline;
                else airConditioning.Online = StateType.Online;
                int model = height >> 4;
                switch (model)
                {
                    case 0:
                        airConditioning.Model = "自动";
                        break;
                    case 1:
                        airConditioning.Model = "制冷";
                        break;
                    case 2:
                        airConditioning.Model = "除湿";
                        break;
                    case 3:
                        airConditioning.Model = "送风";
                        break;
                    case 4:
                        airConditioning.Model = "制热";
                        break;
                }
                airConditioning.Speed = height >> 2;
                airConditioning.SweepWind = height >> 1 == 1 ? "扫风" : "不扫风";
                float val = (0x0f & low);
                airConditioning.Value = val + 16 + "℃";
                airConditioning.Name = "空调";
                airConditioning.Controllable = true;
                airConditioning.Type = type;
                Sensors.Add(airConditioning);
            }
            return index;
        }

        /// <summary>
        /// 处理其他数字量传感器数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">索引</param>
        /// <param name="data">数据</param>
        /// <param name="type">传感器类型</param>
        /// <param name="name">设备名称</param>
        /// <returns></returns>
        private int ProcessOther(int num, int index, byte[] data, int type, string name)
        {
            for (int i = 0; i < num; i++)
            {
                Digital digital = new Digital();
                digital.Id = Convert.ToString(data[index++], 16);
                double value = data[index++];
                digital.Value = value + "";
                digital.Name = name;
                digital.Type = type;
                digital.Online = value == 0 ? StateType.Offline : StateType.Online;
                digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                digital.Controllable = false;
                Sensors.Add(digital);
            }

            return index;
        }

        /// <summary>
        /// 处理光照传感器数据
        /// </summary>
        /// <param name="num">在线数</param>
        /// <param name="index">索引</param>
        /// <param name="data">数据</param>
        /// <param name="type">传感器类型</param>
        /// <returns></returns>
        private int ProcessIllumination(int num, int index, byte[] data, int type)
        {
            for (int i = 0; i < num; i++)
            {
                Digital digital = new Digital();
                digital.Id = Convert.ToString(data[index++], 16);
                double value = data[index++] << 8 | data[index++];
                digital.Value = value + " lx";
                digital.Name = "光照";
                digital.Type = type;
                digital.Online = value == 0 ? StateType.Offline : StateType.Online;
                digital.State = value != 0 ? StateType.StateOpen : StateType.StateClose;
                digital.Controllable = false;
                Sensors.Add(digital);
            }

            return index;
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
        /// 发送转换后的查询命令
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="classroom"></param>
        /// <param name="nodeAdd"></param>
        /// <param name="onoff"></param>
        /// <returns></returns>
        public EquipmentResult SendConvertSearchCmd(byte fun, string classroom)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "00" : classroom;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, 0 };
                byte[] bclassroom = CmdUtils.StrToHexByte(classroom);
                byte[] bnodeAdd = CmdUtils.StrToHexByte("00");
                bclassroom.CopyTo(cmd, 2);
                bnodeAdd.CopyTo(cmd, 5);
                cmd = CmdUtils.ActuatorCommand(cmd);
                SendSearchCmd(cmd);
                oa.Status = true;
                oa.ResultCode = ResultCode.Ok;
            }
            catch (Exception exception)
            {
                oa.Status = false;
                oa.ErrorData = exception.Message;
                oa.ResultCode = ResultCode.Error;
                ExceptionHelper.AddException(exception);
            }
            return oa;
        }
        /// <summary>
        /// 发送转换后的执行命令
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

                if (!ZEquipmentService.CheckClassEquipment(classroom, nodeAdd))
                {
                    throw new EquipmentNoFindException("没有查询到该教室有该ID的设备");
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

                if (!ZEquipmentService.CheckClassEquipment(classroom, nodeAdd))
                {
                    throw new EquipmentNoFindException("没有查询到该教室有该ID的设备");
                }
                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x02, height, low };
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
            }
            return oa;
        }
    }
}