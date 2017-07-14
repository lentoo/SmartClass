using Common;
using Model;
using SmartClass.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using IBLL;
using Model.Properties;
using SmartClass.Models.Enum;
using SmartClass.Models.Filter;
using SmartClass.Models.Types;

namespace SmartClass.Controllers
{
    [EquipmentLogFilter]
    public class HomeController : Controller
    {
        public IZ_EquipmentService ZEquipmentService { get; set; }
        public IZ_RoomService ZRoomService { get; set; }

        public SerialPortService PortService = new SerialPortService();

        /// <summary>
        /// 查询的教室设备节点信息
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="Result">记录结果</param>
        /// <returns>返回记录结果</returns>
        private ClassRoom Search(string classroom,ref EquipmentResult Result)
        {
            //查询该教室
            Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
            if (room == null)   //没有该教室
            {
                Result.Message = "教室地址有误！";
                Result.Message = "查询设备信息失败";
                return null;
            }
            #region 查询教室父级信息
            var rooms = ZRoomService.GetEntity(u => u.F_RoomNo == classroom);
            var allRoom = ZRoomService.GetEntity(u => true);
            var layers = from u in rooms
                         join la in allRoom on u.F_ParentId equals la.F_Id
                         select new
                         {
                             roomName = u.F_FullName,
                             layerName = la.F_FullName,
                             la.F_ParentId,
                             u.F_Id
                         };
            var colleges = (from u in layers
                            join c in allRoom on u.F_ParentId equals c.F_Id
                            select new
                            {
                                collegeName = c.F_FullName,
                                u.layerName,
                                u.roomName,
                                u.F_Id
                            });
            var college = colleges.FirstOrDefault();
            #endregion
            //获取该教室所有的设备
            var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
            byte b;
            byte fun = 0x1f;
            //向串口发送指令
            Result = SendConvertCmd(fun, classroom, "00", 0x00);

            Result.Message = "查询设备信息成功";
            ClassRoom classRoom = PortService.GetReturnData();
            if (classRoom == null)  //如果为空，则在查询一次
            {
                Result = SendConvertCmd(fun, classroom, "00", 0x00);
                classRoom = PortService.GetReturnData();
            }
            if (classRoom != null)
            {
                classRoom.CollegeName = college.collegeName; //学院名称
                classRoom.LayerName = college.layerName;    //楼层名称
                classRoom.Name = college.roomName;          //教室名称
                classRoom.ClassNo = room.F_EnCode;          //教室编码
                classRoom.Id = room.F_RoomNo;
                var list = classRoom.SonserList;
                Result.Count = zeList.Count;
                //var normalZqu = zeList.Where(u => ids.Any(id => u.F_EquipmentNo == id)).ToList();
                var normalZqu = classRoom.SonserList.Where(u => list.Any(l => l.Id == u.Id )).Where(u=>u.Online==StateType.Online).ToList();
                Result.ExceptionCount = zeList.Count - normalZqu.Count;
                Result.NormalCount = Result.Count - Result.ExceptionCount;
            }
            else
            {
                Result.Message = "查询设备信息失败！请重试";
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
            EquipmentResult Result = new EquipmentResult();
            try
            {
                ClassRoom classRoom = Search(classroom,ref Result);
                Result.AppendData = classRoom;
            }
            catch (Exception exception)
            {
                Result.ErrorData = exception.ToString();
                Result.Message = "查询设备信息失败";
            }
            //string data = JsonSerialize.EnSerialize(Result);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询教室异常设备信息
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <returns></returns>
        public ActionResult SearchExc(string classroom)
        {
            EquipmentResult Result = new EquipmentResult();
            try
            {
                ClassRoom classRoom = Search(classroom,ref Result);
                classRoom.SonserList = classRoom.SonserList?.Where(u => u.Online == StateType.Offline).ToList();
                Result.AppendData = classRoom;
            }
            catch (Exception exception)
            {
                Result.Message = "查询设备信息失败";
                Result.ErrorData = exception;
            }
            return Json(Result, JsonRequestBehavior.AllowGet);
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
                byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0x22, 0x01, onoff };
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
                oa.ErrorData = exception.ToString();
                oa.ResultCode = ResultCode.Error;
                ExceptionHelper.AddException(exception);
            }
            return oa;
        }
        /// <summary>
        /// 开关灯
        /// </summary>
        /// <param name="onoff"></param>
        public ActionResult SetLamp(string classroom, string nodeAdd, string onoff)
        {
            EquipmentResult oa = null;
            try
            {
                byte b;
                byte fun = 0x01;
                b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
                oa = SendConvertCmd(fun, classroom, nodeAdd, b);
                oa.Message = "设置灯成功";
            }
            catch
            {
                if (oa != null)
                    oa.Message = "设置灯失败";
            }
            return Json(oa, JsonRequestBehavior.AllowGet);

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
            EquipmentResult oa = null;
            try
            {
                byte b;
                byte fun = 0x02;
                b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
                oa = SendConvertCmd(fun, classroom, nodeAdd, b);
                oa.Message = "设置风机成功";
            }
            catch
            {
                if (oa != null)
                    oa.Message = "设置风机失败";
            }
            return Json(oa, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置门
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">门节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public ActionResult SetDoor(string classroom, string nodeAdd, string onoff)
        {
            EquipmentResult oa = null;
            try
            {
                byte b;
                byte fun = 0x03;
                b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
                oa = SendConvertCmd(fun, classroom, nodeAdd, b);
                oa.Message = "设置门成功";
            }
            catch
            {
                if (oa != null)
                    oa.Message = "设置门失败";
            }
            return Json(oa, JsonRequestBehavior.AllowGet);
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
            EquipmentResult oa = null;
            try
            {
                byte b;
                byte fun = 0x04;
                b = (byte)(onoff == StateType.OPEN ? 0x04 : onoff == StateType.STOP ? 0x05 : 0x00);
                oa = SendConvertCmd(fun, classroom, nodeAdd, b);
                oa.Message = "设置窗户成功";
            }
            catch
            {
                if (oa != null)
                    oa.Message = "设置窗户失败";
            }
            return Json(oa, JsonRequestBehavior.AllowGet);
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
            EquipmentResult oa = null;
            try
            {
                byte b;
                byte fun = 0x05;
                b = (byte)(onoff == StateType.OPEN ? 0x04 : onoff == StateType.STOP ? 0x05 : 0x00);
                oa = SendConvertCmd(fun, classroom, nodeAdd, b);
                oa.Message = "设置窗帘成功";
            }
            catch
            {
                if (oa != null)
                    oa.Message = "设置窗帘失败";
            }
            return Json(oa, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 控制报警灯
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="onoff">开关</param>
        /// <returns></returns>
        public ActionResult SetAlarm(string classroom, string nodeAdd, string onoff)
        {
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x0D, 0, 0x01, 0x01 };
                byte[] bClassroom = CmdUtils.StrToHexByte(classroom);
                bClassroom.CopyTo(cmd, 2);
                byte[] bNodeAdd = CmdUtils.StrToHexByte(nodeAdd);
                bNodeAdd.CopyTo(cmd, 5);
                int Ionoff;
                int.TryParse(onoff, out Ionoff);
                byte bOnoff = (byte)Ionoff;
                cmd[7] = bOnoff;
                cmd = CmdUtils.ActuatorCommand(cmd);
                SerialPortUtils.SendCmd(cmd);

                EquipmentResult oa = new EquipmentResult();
                oa.Status = true;
                oa.Message = "设置报警灯成功";
                oa.ResultCode = ResultCode.Ok;
                return Json(oa);
            }
            catch (Exception exception)
            {
                EquipmentResult oa = new EquipmentResult();
                oa.Status = false;
                oa.Message = "设置报警灯失败";
                oa.ErrorData = exception.ToString();
                oa.ResultCode = ResultCode.Error;
                return Json(oa);
            }
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
        public ActionResult SetAirConditioning(string classroom, string nodeAdd, string onoff, string model, string speed, string wd)
        {
            try
            {
                #region 操作设备逻辑
                byte b = new byte();
                if (onoff == StateType.OPEN)
                {
                    b |= 0x1 << 6;
                }
                else if (onoff == StateType.CLOSE)
                {
                    b |= 0x0 << 6;
                }
                switch (model)
                {
                    case "自动":
                        b |= 0x0 << 3;
                        break;
                    case "制冷":
                        b |= 0x1 << 3;
                        break;
                    case "制热":
                        b |= 0x2 << 3;
                        break;
                    case "抽湿":
                        b |= 0x3 << 3;
                        break;
                    case "送风":
                        b |= 0x4 << 3;
                        break;
                }
                switch (speed)
                {
                    case "0":
                        b |= 0x0 << 1;
                        break;
                    case "1":
                        b |= 0x1 << 1;
                        break;
                    case "2":
                        b |= 0x2 << 1;
                        break;
                    case "3":
                        b |= 0x3 << 1;
                        break;
                }
                // b |= 0x1 << 1;
                b |= 0x01;
                byte b1 = new byte();
                int iwd = Convert.ToInt16(wd);
                iwd |= 0x90;
                b1 |= (byte)iwd;
                //发送命令
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x06, 0, 0x02, b, b1 };
                byte[] bclassroom = CmdUtils.StrToHexByte(classroom);
                bclassroom.CopyTo(cmd, 2);


                byte[] bnodeAdd = CmdUtils.StrToHexByte(nodeAdd);
                bnodeAdd.CopyTo(cmd, 5);
                byte[] data = CmdUtils.ActuatorCommand(cmd);
                SerialPortUtils.SendCmd(data);
                #endregion

                EquipmentResult oa = new EquipmentResult();
                oa.Status = true;
                oa.ResultCode = ResultCode.Ok;
                oa.Message = "设置空调成功";
                return Json(oa);
            }
            catch (Exception exception)
            {
                EquipmentResult oa = new EquipmentResult();
                oa.Status = false;
                oa.Message = "设置空调失败";
                oa.ErrorData = exception.ToString();
                oa.ResultCode = ResultCode.Error;
                return Json(oa);
            }

        }

        /// <summary>
        /// 初始化设备节点
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <returns></returns>
        public ActionResult Init(string classroom)
        {
            EquipmentResult Result = new EquipmentResult();
            try
            {
                byte b = 0x00;
                byte fun = 0x1f;
                Result = SendConvertCmd(fun, classroom, "00", b);
                Result.Message = "查询设备信息成功";
            }
            catch
            {
                if (Result != null)
                    Result.Message = "查询设备信息失败";
            }
            ClassRoom classRoom = PortService.GetReturnData();
            var list = classRoom.SonserList;
            List<Z_Equipment> ZEList = new List<Z_Equipment>();
            Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
            if (room != null)
            {
                foreach (var item in list)
                {
                    Z_Equipment zEquipment = new Z_Equipment();
                    zEquipment.F_Id = Guid.NewGuid().ToString();
                    zEquipment.F_RoomId = room.F_Id;
                    zEquipment.F_EquipmentNo = item.Id;
                    zEquipment.F_FullName = item.Name;
                    zEquipment.F_EquipmentType = item.Type + "";
                    zEquipment.F_EnabledMark = true;
                    ZEList.Add(zEquipment);
                }
            }
            ZEquipmentService.AddEntitys(ZEList);
            Result.AppendData = list;
            //return Json(Result, JsonRequestBehavior.AllowGet);
            return Json(Result);
        }
    }
}
