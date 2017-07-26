using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IBLL;
using Model.Result;
using Common;
using Models.Classes;
using System.Threading;
using System.Web.Helpers;
using Autofac;
using Autofac.Integration.Mvc;
using Common.Cache;
using Common.Exception;
using SmartClass.Models.Types;
using Model;
using Model.Enum;
using SmartClass.Models.Autofac;

namespace SmartClass.Models.Job
{
    /// <summary>
    /// 定时查询所有楼栋消息
    /// </summary>
    public class SearchBuildingAllRoomEquipmentJob : IJob
    {
        private readonly IZ_RoomService ZRoomService;
        private readonly IZ_EquipmentService ZEquipmentService;
        private readonly SerialPortService PortService;
        private readonly IZ_RoomService roomService;
        // private readonly IServiceGetter serviceGetter;
        public readonly ICacheHelper Cache;
        public SearchBuildingAllRoomEquipmentJob(IZ_RoomService ZRoomService, IZ_EquipmentService ZEquipmentService, SerialPortService PortService, IZ_RoomService roomService, ICacheHelper Cache)
        {
            this.ZRoomService = ZRoomService;
            this.ZEquipmentService = ZEquipmentService;
            this.PortService = PortService;
            this.roomService = roomService;
            //this.serviceGetter = serviceGetter;
            this.Cache = Cache;
            //Cache = AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<ICacheHelper>(selectCache);
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var rooms = roomService.GetEntity(u => u.F_RoomType == "Building").ToList();
                foreach (var room in rooms)
                {
                    string jsonList = SearchBuildingAllRoomEquipmentInfo(room.F_FullName);
                    Cache.SetCache(room.F_RoomNo, jsonList, DateTime.Now.AddDays(7));
                }
            }
            catch (Exception exception)
            {
                ExceptionHelper.AddException(exception);
            }
        }
        /// <summary>
        /// 通过楼栋查询所有教室异常设备信息
        /// </summary>
        /// <returns></returns>
        public string SearchBuildingAllRoomEquipmentInfo(string buildingName)
        {
            //查询到该楼栋
            var building = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();
            //查询到该楼栋下所有楼层
            var floors = ZRoomService.GetEntity(u => u.F_ParentId == building.F_Id);
            //查询该楼栋下所有楼层的教室
            var rooms = ZRoomService.GetEntity(r => floors.Any(f => r.F_ParentId == f.F_Id)).ToList();
            List<EquipmentResult> list = new List<EquipmentResult>();
            //遍历所有教室，发送查询命令
            foreach (var item in rooms)
            {
                EquipmentResult result = new EquipmentResult();
                try
                {
                    Thread.Sleep(500);
                    ClassRoom classRoom = Search(item, ref result);

                    if (classRoom != null)
                    {
                        classRoom.AbnormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Offline).ToList();
                        classRoom.NormalSonserList = classRoom.SonserList.Where(u => u.Online == StateType.Online).ToList();
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
            string jsonList = Json.Encode(list);
            return jsonList;

        }
        /// <summary>
        /// 查询的教室设备节点信息
        /// </summary>
        /// <param name="classroom">教室</param>
        /// <param name="result">记录结果</param>
        /// <returns>返回记录结果</returns>
        private ClassRoom Search(Z_Room room, ref EquipmentResult result)
        {
            //获取该教室所有的设备
            var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
            byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Search"));
            //向串口发送指令
            result = PortService.SendConvertCmd(fun, room.F_RoomNo, null, 0x00);

            result.Message = "查询设备信息成功";

            ClassRoom classRoom = PortService.GetReturnData();
            if (classRoom == null)
            {
                Thread.Sleep(1000);
                //向串口发送指令
                result = PortService.SendConvertCmd(fun, room.F_RoomNo, null, 0x00);
                classRoom = PortService.GetReturnData();
            }

            if (classRoom != null)
            {
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
                result.Status = false;
                result.ResultCode = ResultCode.Error;
                result.Message = "查询设备信息失败！请重试";
            }
            return classRoom;
        }
    }
}