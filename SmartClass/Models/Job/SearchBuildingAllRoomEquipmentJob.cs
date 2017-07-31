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
using SmartClass.Models.Classes;

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
        private readonly SearchService searchService;
        // private readonly IServiceGetter serviceGetter;
        public readonly ICacheHelper Cache;
        public SearchBuildingAllRoomEquipmentJob(IZ_RoomService ZRoomService, IZ_EquipmentService ZEquipmentService, SerialPortService PortService, ICacheHelper Cache, SearchService searchService)
        {
            this.ZRoomService = ZRoomService;
            this.ZEquipmentService = ZEquipmentService;
            this.PortService = PortService;
            this.Cache = Cache;
            this.searchService = searchService;
        }

        public void Execute(IJobExecutionContext context)
        {
            Thread.CurrentThread.IsBackground = true;                
            try
            {
                List<Buildings> allBuilding = new List<Buildings>();
                var rooms = ZRoomService.GetEntity(u => u.F_RoomType == "Building").ToList();
                foreach (var room in rooms)
                {
                    //string jsonList = SearchBuildingAllRoomEquipmentInfo(room.F_FullName);

                    //遍历所有教室，发送查询命令
                    //Cache.SetCache(room.F_Id, jsonList, DateTime.Now.AddDays(7));
                    //allBuilding.Add(room);
                    Buildings building = SearchBuildingAllRoomEquipmentInfo1(room.F_FullName, room);
                    allBuilding.Add(building);
                }
                Cache.SetCache("allClassEquipmentInfo", allBuilding, DateTime.Now.AddDays(7));
            }
            catch (Exception exception)
            {
                ExceptionHelper.AddException(exception);
                QuartzConfig.StopJob();
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
        public Buildings SearchBuildingAllRoomEquipmentInfo1(string buildingName, Z_Room room)
        {
            Buildings buid = new Buildings();
            //查询到该楼栋
            var building = room;
            buid.Name = building.F_FullName;

            //查询到该楼栋下所有楼层
            var floors = ZRoomService.GetEntity(u => u.F_ParentId == building.F_Id);

            //查询该楼栋下所有楼层的教室
            var rooms = ZRoomService.GetEntity(r => floors.Any(f => r.F_ParentId == f.F_Id)).ToList();

            foreach (var floor in floors)
            {
                Floors fl = new Floors();
                foreach (var item in rooms)
                {
                    if (item.F_ParentId == floor.F_Id)
                    {
                        EquipmentResult result = new EquipmentResult();
                        ClassRoom classRoom = searchService.Search(item, ref result);
                        if (classRoom != null)
                        {
                            classRoom.LayerName = floor.F_FullName;
                            classRoom.BuildingName = building.F_FullName;
                            classRoom.Name = item.F_FullName;
                            classRoom.ClassNo = item.F_RoomNo;
                            if (classRoom.AbnormalSonserList.Count > 0)
                            {
                                buid.ExceptionCount +=1;
                                buid.AbnormalEquipment = true;
                                fl.ExceptionCount +=1;
                                fl.AbnormalEquipment = true;
                                classRoom.AbnormalEquipment = true;
                                classRoom.ExceptionCount = classRoom.AbnormalSonserList.Count;
                            }
                        }
                        fl.ClassRooms.Add(classRoom);
                        result.AppendData = classRoom;
                    }
                }
                fl.Name = floor.F_FullName;
                buid.Floors.Add(fl);
            }
            return buid;
        }

    }
}