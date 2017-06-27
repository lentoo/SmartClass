using Autofac;
using Autofac.Integration.Mvc;

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

namespace SmartClass.Controllers
{
    [MyActionFilter]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 查询教室数据
        /// </summary>
        /// <param name="Room">教室地址</param>
        /// <returns></returns>
        public ActionResult Process(string classroom)
        {
            byte[] Cmd = new byte[] { 0x55, 0x02, 0x12, 0x34, 0x1f, 0x00, 0x01, 0x00 };
            byte[] roomId = CmdUtils.StrToHexByte(classroom);
            roomId.CopyTo(Cmd, 2);
            Cmd = CmdUtils.ActuatorCommand(Cmd);
            if (!SerialPortUtils.SendCmd(Cmd))
            {
            }
            Thread.Sleep(300);
            return Json(new { SerialPortUtils.actuators, result = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置空调参数
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="onoff">开关</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <param name="model">模式</param>
        /// <param name="speed">风速</param>
        /// <param name="wd">温度</param>
        /// <returns></returns>
        public ActionResult SetAirConditioning(string classroom, string onoff, string nodeAdd, string model, string speed, string wd)
        {
            try
            {
                byte b = new byte();
                if (onoff == "01")
                {
                    b |= 0x1 << 7;
                }
                else if (onoff == "00")
                {
                    b |= 0x0 << 7;
                }
                switch (model)
                {
                    case "自动":
                        b |= 0x0 << 4;
                        break;
                    case "制冷":
                        b |= 0x1 << 4;
                        break;
                    case "制热":
                        b |= 0x2 << 4;
                        break;
                    case "抽湿":
                        b |= 0x3 << 4;
                        break;
                    case "送风":
                        b |= 0x4 << 4;
                        break;
                }
                switch (speed)
                {
                    case "0":
                        b |= 0x0 << 2;
                        break;
                    case "1":
                        b |= 0x1 << 2;
                        break;
                    case "2":
                        b |= 0x2 << 2;
                        break;
                    case "3":
                        b |= 0x3 << 2;
                        break;
                }
                b |= 0x1 << 1;

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
                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置空调成功";
                return Json(oa);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置空调失败";
                return Json(oa);
            }

        }
        /// <summary>
        /// 开关灯
        /// </summary>
        /// <param name="onoff"></param>
        public ActionResult Lamp(string classroom, string nodeAdd, int onoff)
        {
            try
            {
                byte[] cmd = { 0x55, 0x02, 0x12, 0x34, 0x01, 0x22, 0x01, 0x01 };
                byte[] bclassroom = CmdUtils.StrToHexByte(classroom);
                byte[] bnodeAdd = CmdUtils.StrToHexByte(nodeAdd);
                byte bonoff = (byte)onoff;
                bclassroom.CopyTo(cmd, 2);
                bnodeAdd.CopyTo(cmd, 5);
                cmd[7] = bonoff;//.CopyTo(cmd, 7);
                cmd = CmdUtils.ActuatorCommand(cmd);
                SerialPortUtils.SendCmd(cmd);
                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置灯成功";
                return Json(oa,JsonRequestBehavior.AllowGet);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置灯失败";
                return Json(oa, JsonRequestBehavior.AllowGet);
            }
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
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x0A, 0x22, 0x01, 0x01 };
                byte[] classAdd = CmdUtils.StrToHexByte(classroom);
                classAdd.CopyTo(cmd, 2);
                byte[] node = CmdUtils.StrToHexByte(nodeAdd);
                node.CopyTo(cmd, 5);
                byte[] bonoff = CmdUtils.StrToHexByte(onoff);
                bonoff.CopyTo(cmd, 7);
                cmd = CmdUtils.ActuatorCommand(cmd);
                SerialPortUtils.SendCmd(cmd);
                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置门成功";
                return Json(oa);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置门失败";
                return Json(oa);
            }

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
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x0C, 0, 0x01, 0x01 };
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

                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置窗帘成功";
                return Json(oa);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置窗帘失败";
                return Json(oa);
            }
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
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x0B, 0, 0x01, 0x01 };
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

                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置窗户成功";
                return Json(oa);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置窗户失败";
                return Json(oa);
            }
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

                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置报警灯成功";
                return Json(oa);
            }
            catch
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置报警灯失败";
                return Json(oa);
            }
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
            try
            {
                byte[] cmd = { 0x55, 0x02, 0, 0, 0x09, 0, 0x01, 0x01 };
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
                OperationActuator oa = new OperationActuator();
                oa.Status = true;
                oa.Message = "设置风机成功";
                return Json(oa);
            }
            catch (Exception)
            {
                OperationActuator oa = new OperationActuator();
                oa.Status = false;
                oa.Message = "设置风机失败";
                return Json(oa);
            }
        }
    }
}
