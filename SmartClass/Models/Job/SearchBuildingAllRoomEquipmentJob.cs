using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartClass.IService;
using System.Threading;
using SmartClass.Infrastructure.Cache;
using SmartClass.Infrastructure.Exception;
using Model;
using Model.AutoMapperConfig;
using Model.DTO.Classes;
using Model.DTO.Result;


namespace SmartClass.Models.Job
{
  /// <summary>
  /// 定时查询所有楼栋消息
  /// </summary>
  public class SearchBuildingAllRoomEquipmentJob : IJob
  {
    private readonly IZ_RoomService ZRoomService;
    private readonly SerialPortService PortService;
    private readonly ICacheHelper Cache;
    public SearchBuildingAllRoomEquipmentJob(IZ_RoomService ZRoomService, SerialPortService PortService, ICacheHelper Cache)
    {
      this.ZRoomService = ZRoomService;
      this.PortService = PortService;
      this.Cache = Cache;
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
          Buildings building = SearchBuildingAllRoomEquipmentInfo1(room.F_FullName, room);
          allBuilding.Add(building);
        }
        Cache.SetCache("allClassEquipmentInfo", allBuilding, DateTime.Now.AddDays(7));
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
    //public string SearchBuildingAllRoomEquipmentInfo(string buildingName)
    //{
    //    //查询到该楼栋
    //    var building = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();
    //    //查询到该楼栋下所有楼层
    //    var floors = ZRoomService.GetEntity(u => u.F_ParentId == building.F_Id);
    //    //查询该楼栋下所有楼层的教室
    //    var rooms = ZRoomService.GetEntity(r => floors.Any(f => r.F_ParentId == f.F_Id)).ToList();

    //    List<EquipmentResult> list = new List<EquipmentResult>();

    //    foreach (var item in rooms)
    //    {
    //        EquipmentResult result = new EquipmentResult();
    //        try
    //        {
    //            ClassRoom classRoom = searchService.Search(item, ref result);
    //            result.AppendData = classRoom;
    //            list.Add(result);
    //        }
    //        catch (Exception exception)
    //        {
    //            result.ErrorData = exception.ToString();
    //            result.Message = "查询设备信息失败";
    //        }
    //    }

    //    string jsonList = Json.Encode(list);
    //    return jsonList;
    //}

    /// <summary>
    /// 通过楼栋查询所有教室设备信息
    /// </summary>
    /// <param name="buildingName"></param>
    /// <param name="room"></param>
    /// <returns></returns>
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
            ClassRoom classRoom = PortService.Search(item, ref result);
            if (classRoom != null)
            {
              AutoMapperConfig.Map(building, classRoom);
              AutoMapperConfig.Map(floor, classRoom);
              AutoMapperConfig.Map(item, classRoom);
              //classRoom.LayerName = floor.F_FullName;
              //classRoom.BuildingName = building.F_FullName;
              //classRoom.Name = item.F_FullName;
              //classRoom.ClassNo = item.F_EnCode;
              if (classRoom.AbnormalSonserList.Count > 0)
              {
                buid.ExceptionCount += 1;
                buid.AbnormalEquipment = true;
                fl.ExceptionCount += 1;
                fl.AbnormalEquipment = true;
                classRoom.AbnormalEquipment = true;
                classRoom.ExceptionCount = classRoom.AbnormalSonserList.Count;
              }
            }
            else
            {
              classRoom = new ClassRoom();
              AutoMapperConfig.Map(building, classRoom);
              AutoMapperConfig.Map(floor, classRoom);
              AutoMapperConfig.Map(item, classRoom);
              //classRoom.LayerName = floor.F_FullName;
              //classRoom.BuildingName = building.F_FullName;
              //classRoom.Name = item.F_FullName;
              //classRoom.ClassNo = item.F_EnCode;
              //classRoom.Id = item.F_RoomNo;
            }
            fl.ClassRooms.Add(classRoom);
            result.AppendData = classRoom;
          }
        }
        if (fl.ClassRooms.Count == 0)
        {
          fl.Name = floor.F_FullName + " (没有教室)";
        }
        else
        {
          fl.Name = floor.F_FullName;
        }
        buid.Floors.Add(fl);
      }
      return buid;
    }

  }
}