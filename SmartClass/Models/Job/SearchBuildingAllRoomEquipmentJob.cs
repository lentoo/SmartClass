using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly SearchService searchService;
        // private readonly IServiceGetter serviceGetter;
        public readonly ICacheHelper Cache;
        public SearchBuildingAllRoomEquipmentJob(IZ_RoomService ZRoomService, IZ_EquipmentService ZEquipmentService, SerialPortService PortService, IZ_RoomService roomService, ICacheHelper Cache, SearchService searchService)
        {
            this.ZRoomService = ZRoomService;
            this.ZEquipmentService = ZEquipmentService;
            this.PortService = PortService;
            this.roomService = roomService;
            this.Cache = Cache;
            this.searchService = searchService;
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var rooms = roomService.GetEntity(u => u.F_RoomType == "Building").ToList();
                foreach (var room in rooms)
                {
                    string jsonList = SearchBuildingAllRoomEquipmentInfo(room.F_FullName);
                    //遍历所有教室，发送查询命令
                    Cache.SetCache(room.F_Id, jsonList, DateTime.Now.AddDays(7));
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
           
            foreach (var item in rooms)
            {
                EquipmentResult result = new EquipmentResult();
                try
                {
                    ClassRoom classRoom = searchService.Search(item, ref result);
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

    }
}