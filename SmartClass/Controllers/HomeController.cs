using Common;
using Model;
using SmartClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IBLL;
using Model.Enum;
using Model.Result;
using SmartClass.Models.Exceptions;
using SmartClass.Models.Filter;
using SmartClass.Models.Types;
using System.Diagnostics;
using System.Configuration;
using Models.Classes;
using System.Threading;

namespace SmartClass.Controllers
{
    /// <summary>
    /// 控制设备状态  控制器
    /// </summary>
    [EquipmentLogFilter]
    public class HomeController : Controller
    {
        /// <summary>
        /// 设备服务
        /// </summary>
        public IZ_EquipmentService ZEquipmentService { get; set; }
        /// <summary>
        /// 课室服务
        /// </summary>
        public IZ_RoomService ZRoomService { get; set; }
        /// <summary>
        /// 串口服务
        /// </summary>
        public SerialPortService PortService = new SerialPortService();
        /// <summary>
        /// 操作设备结果
        /// </summary>
        EquipmentResult _oa = new EquipmentResult();
        /// <summary>
        /// 查询的教室设备节点信息
        /// </summary>
        /// <param name="classroom">教室</param>
        /// <param name="result">记录结果</param>
        /// <returns>返回记录结果</returns>
        [NonAction]
        private ClassRoom Search(Z_Room room, ref EquipmentResult result)
        {

            #region 查询教室父级信息
            //var rooms = ZRoomService.GetEntity(u => u.F_RoomNo == classroom);
            //var allRoom = ZRoomService.GetEntity(u => true);
            //var layers = from u in rooms
            //             join la in allRoom on u.F_ParentId equals la.F_Id
            //             select new
            //             {
            //                 roomName = u.F_FullName,
            //                 layerName = la.F_FullName,
            //                 la.F_ParentId,
            //                 u.F_Id
            //             };
            //var colleges = (from u in layers
            //                join c in allRoom on u.F_ParentId equals c.F_Id
            //                select new
            //                {
            //                    collegeName = c.F_FullName,
            //                    u.layerName,
            //                    u.roomName,
            //                    u.F_Id
            //                });
            //var college = colleges.FirstOrDefault();
            #endregion

            //获取该教室所有的设备
            var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Search"));
            //向串口发送指令
            result = SendConvertCmd(fun, room.F_RoomNo, null, 0x00);

            result.Message = "查询设备信息成功";

            ClassRoom classRoom = PortService.GetReturnData();
            if (classRoom == null)
            {
                Thread.Sleep(1000);
                //向串口发送指令
                result = SendConvertCmd(fun, room.F_RoomNo, null, 0x00);
            }
            if (classRoom != null)
            {
                //classRoom.CollegeName = college?.collegeName; //学院名称
                //classRoom.LayerName = college?.layerName;    //楼层名称
                //classRoom.Name = college?.roomName;          //教室名称
                classRoom.Name = room?.F_FullName;
                classRoom.ClassNo = room.F_EnCode;          //教室编码
                classRoom.Id = room.F_RoomNo;
                var list = classRoom.SonserList;
                classRoom.AbnormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Offline).ToList();
                classRoom.NormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Online).ToList();
                result.Count = zeList.Count;
                result.ExceptionCount = classRoom.AbnormalSonserList.Count;
                result.NormalCount = classRoom.NormalSonserList.Count;
            }
            else
            {
                result.Message = "查询设备信息失败！请重试";
            }
            return classRoom;
        }

        /// <summary>
        /// 查询教室所有设备数据
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <returns></returns>
        public ActionResult SearchAll(string classroom)
        {
            EquipmentResult result = new EquipmentResult();
            try
            {
                //查询该教室
                Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
                if (room == null)   //没有该教室
                {
                    result.Message = "教室地址有误！";
                }
                else
                {
                    ClassRoom classRoom = Search(room, ref result);
                    result.AppendData = classRoom;
                }
            }
            catch (Exception exception)
            {
                result.ErrorData = exception.ToString();
                result.Message = "查询设备信息失败";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 数据变化测试
        /// </summary>
        /// <returns></returns>
        [EquipmentLogFilter(isCheck = false)]
        [HttpPost]
        public ActionResult SearchTest(string classroom)
        {
            EquipmentResult result = new EquipmentResult();
            Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
            //获取该教室所有的设备
            var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Search"));
            ClassRoom classRoom = PortService.GetReturnDataTest();
            if (classRoom != null)
            {
                //classRoom.CollegeName = college?.collegeName; //学院名称
                //classRoom.LayerName = college?.layerName;    //楼层名称
                //classRoom.Name = college?.roomName;          //教室名称
                classRoom.Name = room?.F_FullName;
                classRoom.ClassNo = room.F_EnCode;          //教室编码
                classRoom.Id = room.F_RoomNo;
                var list = classRoom.SonserList;
                classRoom.AbnormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Offline).ToList();
                classRoom.NormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Online).ToList();
                result.Count = zeList.Count;
                result.ExceptionCount = classRoom.AbnormalSonserList.Count;
                result.NormalCount = classRoom.NormalSonserList.Count;
                result.AppendData = classRoom;
            }
            else
            {
                result.Message = "查询设备信息失败！请重试";
            }
            return Json(result);
        }
        /// <summary>
        /// 发送转换后的命令
        /// </summary>
        /// <param name="fun">功能码</param>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        private EquipmentResult SendConvertCmd(byte fun, string classroom, string nodeAdd, byte onoff)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "00" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                if (nodeAdd != "00")    //节点地址00表示查询所有设备信息
                {
                    if (!CheckClassEquipment(classroom, nodeAdd))
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

                PortService.SendCmd(cmd);
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
        private EquipmentResult SendConvertCmd(byte fun, string classroom, string nodeAdd, byte height, byte low)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "00" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                if (nodeAdd != "00")
                {
                    if (!CheckClassEquipment(classroom, nodeAdd))
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

                PortService.SendCmd(cmd);
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

        /// <summary>
        /// 开关灯
        /// </summary>
        /// <param name="classroom"></param>
        /// <param name="nodeAdd"></param>
        /// <param name="onoff"></param>
        /// <returns></returns>
        public ActionResult SetLamp(string classroom, string nodeAdd, string onoff)
        {
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Lamp"));
            byte b;
            // byte fun = 0x01;
            b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置灯成功" : "设置灯失败";
            return Json(_oa, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 设置风机
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public ActionResult SetFan(string classroom, string nodeAdd, string onoff)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "0000" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Fan"));
            byte b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置风机成功" : "设置风机失败";
            return Json(_oa, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置门
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">门节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetDoor(string classroom, string nodeAdd, string onoff)
        {
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Door"));
            byte b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置门成功" : "设置门失败";
            return Json(_oa);
        }
        /// <summary>
        /// 设置窗户
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public ActionResult SetWindow(string classroom, string nodeAdd, string onoff)
        {
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Window"));
            byte b = (byte)(onoff == StateType.OPEN ? 0x04 : onoff == StateType.STOP ? 0x05 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置窗户成功" : "设置窗户失败";
            return Json(_oa, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置窗帘
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public ActionResult SetCurtain(string classroom, string nodeAdd, string onoff)
        {
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Curtain"));
            byte b = (byte)(onoff == StateType.OPEN ? 0x04 : onoff == StateType.STOP ? 0x05 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置窗帘成功" : "设置窗帘失败";
            return Json(_oa, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 设置空调参数
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <param name="model">模式</param>
        /// <param name="speed">风速</param>
        /// <param name="wd">温度</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetAirConditioning(string classroom, string nodeAdd, string onoff, string model, string speed, string wd)
        {
            byte height = (byte)(onoff == StateType.OPEN ? 1 << 7 : 0 << 7);
            Int16 m = Convert.ToInt16(model);
            height |= (byte)(m << 4);
            Int16 s = Convert.ToInt16(speed);
            height |= (byte)(s << 2);
            height |= 0x01;
            byte low = (byte)Convert.ToInt16(wd);
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Air"));
            _oa = SendConvertCmd(fun, classroom, nodeAdd, height, low);
            _oa.Message = _oa.Status ? "设置空调成功" : "设置空调失败";
            return Json(_oa);
        }

        /// <summary>
        /// 设置电子钟
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="time">时间 格式：2017/7/25 15:16:30</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetElectronicClock(string classroom, string nodeAdd)
        {
            classroom = string.IsNullOrEmpty(classroom) ? "0000" : classroom;
            nodeAdd = string.IsNullOrEmpty(nodeAdd) ? "00" : nodeAdd;

            EquipmentResult oa = new EquipmentResult();
            try
            {
                byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Clock"));

                byte[] classAddr = CmdUtils.StrToHexByte(classroom);
                byte[] nodeAddr = CmdUtils.StrToHexByte(nodeAdd);
                DateTime currentTime = DateTime.Now;                //获取当前时间
                                                                    //转换时间格式
                string year = (currentTime.Year % 100).ToString();
                string month = currentTime.Month < 10 ? "0" + currentTime.Month : currentTime.Month.ToString();
                string day = currentTime.Day < 10 ? "0" + currentTime.Day : currentTime.Day.ToString();
                string hour = currentTime.Hour < 10 ? "0" + currentTime.Hour : currentTime.Hour.ToString();
                string minute = currentTime.Minute < 10 ? "0" + currentTime.Minute : currentTime.Minute.ToString();
                string second = currentTime.Second < 10 ? "0" + currentTime.Second : currentTime.Second.ToString();
                string date = $"{year} {month} {day}";      //日期部分
                byte[] yMd = CmdUtils.StrToHexByte(date);   //将日期部分转为byte[]类型
                string time = $"{hour} {minute} {second}";  //时间部分
                byte[] hms = CmdUtils.StrToHexByte(time);   //将时间部分转为byte[]类型
                byte week = (byte)(Convert.ToInt32(currentTime.DayOfWeek.ToString("d")));

                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x0D, 0, 0, 0, week, 0, 0, 0, 0x23, 0, 0, 0x70, 0, 0 };
                classAddr.CopyTo(cmd, 2);
                nodeAddr.CopyTo(cmd, 5);
                yMd.CopyTo(cmd, 7);
                hms.CopyTo(cmd, 11);
                cmd = CmdUtils.ActuatorCommand(cmd);
                PortService.SendCmd(cmd);
                oa.Status = true;
                oa.Message = "时间同步成功";
                oa.ResultCode = ResultCode.Ok;
            }
            catch (Exception exception)
            {
                oa.Status = false;
                oa.ResultCode = ResultCode.Error;
                oa.Message = "时间同步失败";
                oa.ErrorData = exception.Message;
            }
            return Json(oa);
        }
        /// <summary>
        /// 设置投影屏状态
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetProjectionScreen(string classroom, string nodeAdd, string onoff)
        {
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("ProjectionScreen"));
            byte b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
            _oa = SendConvertCmd(fun, classroom, nodeAdd, b);
            _oa.Message = _oa.Status ? "设置投影屏成功" : "设置投影屏失败";
            return Json(_oa);
        }

        /// <summary>
        /// 初始化设备节点
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <returns></returns>
        public ActionResult Init(string classroom)
        {
            byte b = 0x00;
            byte fun = 0x1f;
            _oa = SendConvertCmd(fun, classroom, "00", b);
            _oa.Message = _oa.Status ? "查询设备信息成功" : "查询设备信息失败";
            ClassRoom classRoom = PortService.GetReturnData();
            var list = classRoom.SonserList;
            List<Z_Equipment> zeList = new List<Z_Equipment>();
            Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
            if (room != null)
            {
                foreach (var item in list)
                {
                    Z_Equipment zEquipment = new Z_Equipment
                    {
                        F_Id = Guid.NewGuid().ToString(),
                        F_RoomId = room.F_Id,
                        F_EquipmentNo = item.Id,
                        F_FullName = item.Name,
                        F_EquipmentType = item.Type + "",
                        F_EnabledMark = true
                    };
                    zeList.Add(zEquipment);
                }
            }
            ZEquipmentService.AddEntitys(zeList);
            _oa.AppendData = list;
            //return Json(Result, JsonRequestBehavior.AllowGet);
            return Json(_oa, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 按楼栋控制设备
        /// </summary>
        /// <param name="buildingName">楼栋名称</param>
        /// <param name="equipmentType">设备类型</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>        
        [HttpPost]
        public ActionResult ControlBuildingEquipment(string buildingName, string equipmentType, string onoff)
        {
            EquipmentResult oa = new EquipmentResult();
            try
            {
                Z_Room room = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();//查找该楼栋
                var floors = ZRoomService.GetEntity(u => u.F_ParentId == room.F_Id);     //该楼栋所有楼层
                                                                                         //楼层中所有教室
                var classroom = ZRoomService.GetEntity(u => floors.Any(f => f.F_Id == u.F_ParentId));
                var equis = ZEquipmentService.GetEntity(e => classroom.Any(r => e.F_RoomId == r.F_Id)).Where(e => e.F_EquipmentType == equipmentType);
                //筛选出教室编码和设备编码
                var val = (from c in classroom
                           join e in equis on c.F_Id equals e.F_RoomId
                           select new
                           {
                               c.F_RoomNo,
                               e.F_EquipmentNo
                           }).ToList();
                List<byte> listByte = new List<byte>();
                foreach (var item in val)
                {
                    byte fun = (byte)GetFunByEquipmentType(equipmentType);
                    byte b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
                    byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, b };
                    byte[] bclassroom = CmdUtils.StrToHexByte(item.F_RoomNo);
                    byte[] bnodeAdd = CmdUtils.StrToHexByte(item.F_EquipmentNo);
                    bclassroom.CopyTo(cmd, 2);
                    bnodeAdd.CopyTo(cmd, 5);
                    cmd = CmdUtils.ActuatorCommand(cmd);
                    listByte.AddRange(cmd);
                }
                PortService.SendCmd(listByte.ToArray());
                oa.Message = "控制整栋楼层设备成功";
                oa.ResultCode = ResultCode.Ok;
                oa.Status = true;
            }
            catch (Exception ex)
            {
                oa.Message = "控制整栋楼层设备失败";
                oa.ResultCode = ResultCode.Error;
                oa.Status = false;
                oa.ErrorData = ex.Message;
            }
            return Json(oa);
        }
        /// <summary>
        /// 控制楼栋中楼层设备
        /// </summary>
        /// <param name="buildingName">楼栋名称</param>
        /// <param name="floorName">楼层名称</param>
        /// <param name="equipmentType">设备类型</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ControlFloorEquipment(string buildingName, string floorName, string equipmentType, string onoff)
        {
            Z_Room room = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();//查找该楼栋
            var floor = ZRoomService.GetEntity(u => u.F_ParentId == room.F_Id).Where(u => u.F_FullName == floorName);     //该楼栋的该楼层
            var classroom = ZRoomService.GetEntity(u => floor.Any(f => f.F_Id == u.F_ParentId));  //楼层中所有教室
            //查询该楼层下的该设备型号的所有设备
            var equis = ZEquipmentService.GetEntity(e => classroom.Any(r => e.F_RoomId == r.F_Id)).Where(e => e.F_EquipmentType == equipmentType);
            //筛选出教室编码和设备编码
            var val = (from c in classroom
                       join e in equis on c.F_Id equals e.F_RoomId
                       select new
                       {
                           c.F_RoomNo,
                           e.F_EquipmentNo
                       }).ToList();
            List<byte> listByte = new List<byte>();
            foreach (var item in val)
            {
                byte fun = (byte)GetFunByEquipmentType(equipmentType);
                byte b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, b };
                byte[] bclassroom = CmdUtils.StrToHexByte(item.F_RoomNo);
                byte[] bnodeAdd = CmdUtils.StrToHexByte(item.F_EquipmentNo);
                bclassroom.CopyTo(cmd, 2);
                bnodeAdd.CopyTo(cmd, 5);
                cmd = CmdUtils.ActuatorCommand(cmd);
                listByte.AddRange(cmd);
            }
            PortService.SendCmd(listByte.ToArray());
            return Json(val);
        }

        /// <summary>
        /// 通过楼栋查询所有教室异常设备信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchBuildingAllRoomEquipmentInfo(string buildingName)
        {
            //查询到该楼栋
            var building = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();
            //查询到该楼栋下所有楼层
            var floors = ZRoomService.GetEntity(u => u.F_ParentId == building.F_Id);
            //查询该楼栋下所有楼层的教室
            var rooms = ZRoomService.GetEntity(r => floors.Any(f => r.F_ParentId == f.F_Id)).ToList();
            List<EquipmentResult> list = new List<EquipmentResult>();
            byte fun = (byte)(Convert.ToInt32(ConfigurationManager.AppSettings["Search"]));
            //遍历所有教室，发送查询命令
            foreach (var item in rooms)
            {
                byte[] b = { 0x55, 0x02, 0x00, fun, 0x00, 0x01, 0x00 };
                byte[] classAddr = CmdUtils.StrToHexByte(item.F_RoomNo);

                EquipmentResult result = new EquipmentResult();
                try
                {
                    ClassRoom classRoom = Search(item, ref result);
                    Thread.Sleep(500);
                    if (classRoom != null)
                    {
                        classRoom.SonserList = classRoom?.SonserList?.Where(u => u.Online == StateType.Offline)?.ToList();
                    }
                    result.AppendData = classRoom;
                    list.Add(result);
                }
                catch (Exception exception)
                {
                    result.ErrorData = exception.ToString();
                    result.Message = "查询设备信息失败";
                }
            }
            return Json(list);
        }

        /// <summary>
        /// 通过设备类型获取功能码
        /// </summary>
        /// <param name="equipmentType">设备类型</param>
        /// <returns></returns>
        [NonAction]
        private int GetFunByEquipmentType(string equipmentType)
        {
            string fun = string.Empty;
            var settings = ConfigurationManager.AppSettings;
            switch (equipmentType)
            {
                case "1":
                    fun = settings["Lamp"];
                    break;
                case "2":
                    fun = settings["Door"];
                    break;
                case "3":
                    fun = settings["Curtain"];
                    break;
                case "4":
                    fun = settings["Window"];
                    break;
                case "12":
                    fun = settings["Air"];
                    break;
            }
            //Convert.ToInt32(fun);
            return Convert.ToInt32(fun);
        }

        /// <summary>
        /// 校验该教室是否有该设备
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <returns>true or false</returns>
        [NonAction]
        private bool CheckClassEquipment(string classroom, string nodeAdd)
        {
            string node = Convert.ToInt32(nodeAdd) + "";
            var room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom);
            var equipment = ZEquipmentService.GetEntity(u => u.F_EquipmentNo == node);
            var roomEquipment =
            (from r in room
             join e in equipment on r.F_Id equals e.F_RoomId
             select new
             {
                 classroom = r.F_RoomNo,
                 className = r.F_FullName,
                 nodeAdd = e.F_EquipmentNo,
                 nodeName = e.F_FullName
             }).FirstOrDefault();
            if (roomEquipment != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
