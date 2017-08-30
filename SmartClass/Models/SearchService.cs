using Model;
using System;
using System.Linq;
using SmartClass.IService;
using SmartClass.Infrastructure;
using System.Threading;
using Model.DTO.Classes;
using Model.DTO.Result;
using SmartClass.Models.Types;
using Model.Enum;

namespace SmartClass.Models
{
    /// <summary>
    /// 查询服务类
    /// </summary>
    public class SearchService
    {
        private readonly IZ_EquipmentService ZEquipmentService;
        private readonly SerialPortService PortService;
        public SearchService(IZ_EquipmentService ZEquipmentService, SerialPortService PortService)
        {
            this.ZEquipmentService = ZEquipmentService;
            this.PortService = PortService;
        }
        /// <summary>
        /// 查询的教室设备节点信息
        /// </summary>
        /// <param name="classroom">教室</param>
        /// <param name="result">记录结果</param>
        /// <returns>返回记录结果</returns>
        public ClassRoom Search(Z_Room room,ref EquipmentResult result)
        {
            //获取该教室所有的设备
            var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Search"));
            //向串口发送查询指令
            result = PortService.SendConvertSearchCmd(fun, room.F_RoomNo);

            result.Message = "查询设备信息成功";

            ClassRoom classRoom = PortService.GetReturnData(room.F_RoomNo);
            if (classRoom == null) //没有数据就重新发一次
            {
                Thread.Sleep(500);
                //向串口发送指令
                result = PortService.SendConvertSearchCmd(fun, room.F_RoomNo);
                classRoom = PortService.GetReturnData(room.F_RoomNo);
            }
            if (classRoom != null)
            {
                classRoom.Name = room.F_FullName;
                classRoom.ClassNo = room.F_EnCode;          //教室编码
                classRoom.Id = room.F_RoomNo;
                classRoom.AbnormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Offline).ToList();
                classRoom.NormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Online).ToList();
                result.Count = zeList.Count;
                result.ExceptionCount = zeList.Count - classRoom.NormalSonserList.Count;
                result.NormalCount = classRoom.NormalSonserList.Count;
            }
            else
            {
                result.Status = false;
                result.ResultCode = ResultCode.Error;
                result.Message = "查询设备信息失败！请重试";
            }
            return classRoom;
        }
    }
}