using SmartClass.Infrastructure;
using Model;
using SmartClass.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartClass.IService;
using Model.Enum;
using SmartClass.Models.Filter;
using SmartClass.Models.Types;
using System.Configuration;
using SmartClass.Infrastructure.Cache;
using SmartClass.Infrastructure.Exception;
using SmartClass.Infrastructure.Extended;
using Model.DTO;
using Model.DTO.Classes;
using Model.DTO.Result;
using SmartClass.Models.Enum;

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
    public ICacheHelper Cache { get; set; }
    /// <summary>
    /// 课室服务
    /// </summary>
    public IZ_RoomService ZRoomService { get; set; }
    /// <summary>
    /// 串口服务
    /// </summary>
    public SerialPortService PortService { get; set; }
    public SearchService searchService { get; set; }
    /// <summary>
    /// 操作设备结果
    /// </summary>
    EquipmentResult EResult = new EquipmentResult();

    /// <summary>
    /// 查询教室所有设备数据
    /// </summary>
    /// <param name="queryParams">查询参数</param>
    /// <returns></returns>
    [OutputCache(Duration = 30)]
    public ActionResult SearchAll(QueryParams queryParams)
    {
      EquipmentResult result = new EquipmentResult();
      try
      {
        //查询该教室
        Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == queryParams.classroom).FirstOrDefault();

        if (room == null)   //没有该教室
        {
          result.Message = "教室地址有误！";
        }
        else
        {
          ClassRoom classRoom = searchService.Search(room, ref result);

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
    public ActionResult SearchTest(QueryParams queryParams)
    {

      EquipmentResult result = new EquipmentResult();
      Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == queryParams.classroom).FirstOrDefault();
      //获取该教室所有的设备
      var zeList = ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList();
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Search"));
      ClassRoom classRoom = PortService.GetReturnDataTest(queryParams.classroom);
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
        result.AppendData = classRoom;
      }
      else
      {
        result.Message = "查询设备信息失败！请重试";
      }
      return Json(result);
    }

    /// <summary>
    /// 设置灯
    /// </summary>
    /// <param name="controlParams"></param>
    /// <returns></returns>
    public ActionResult SetLamp(ControlParams controlParams)
    {
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Lamp"));

      int res = controlParams.nodeAdd.IndexOf('_');
      if (res != -1)      //表示一个灯节点控制多个灯
      {
        int state = Cache.GetCache<int>(controlParams.classroom);
        string[] node = controlParams.nodeAdd.Split('_'); //下滑线后面表示控制第几个灯
        if (node[1] == "1")                 //控制第1个灯
        {
          if (controlParams.onoff == StateType.CLOSE)
          {
            state &= ~0x02;
          }
          else
          {
            state |= 0x02;
          }
        }
        else if (node[1] == "0")             //控制第0个灯
        {

          if (controlParams.onoff == StateType.CLOSE)
          {
            state &= ~0x01;
          }
          else
          {
            state |= 0x01;
          }
        }
        Cache.SetCache(controlParams.classroom, state);
        EResult = PortService.SendConvertCmd(fun, controlParams.classroom, node[0], (byte)state);
      }
      else                //表示一个灯节点控制一个灯
      {
        byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x01 : 0x00);
        EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      }
      EResult.Message = EResult.Status ? "设置灯成功" : "设置灯失败";
      return Json(EResult);
    }

    /// <summary>
    /// 设置风机
    /// </summary>
    /// <param name="classroom">教室地址</param>
    /// <param name="nodeAdd">节点地址</param>
    /// <param name="onoff">开关</param>
    /// <returns></returns>
    public ActionResult SetFan(ControlParams controlParams)
    {
      controlParams.classroom = string.IsNullOrEmpty(controlParams.classroom) ? "0000" : controlParams.classroom;
      controlParams.nodeAdd = string.IsNullOrEmpty(controlParams.nodeAdd) ? "00" : controlParams.nodeAdd;
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Fan"));
      byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x01 : 0x00);
      EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      EResult.Message = EResult.Status ? "设置风机成功" : "设置风机失败";
      return Json(EResult, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// 设置门
    /// </summary>
    /// <param name="controlParams"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SetDoor(ControlParams controlParams)
    {
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Door"));
      byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x01 : 0x00);
      EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      EResult.Message = EResult.Status ? "设置门成功" : "设置门失败";
      return Json(EResult);
    }

    /// <summary>
    /// 设置窗户
    /// </summary>
    /// <param name="controlParams"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SetWindow(ControlParams controlParams)
    {
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Window"));
      byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x04 : controlParams.onoff == StateType.STOP ? 0x05 : 0x00);
      EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      EResult.Message = EResult.Status ? "设置窗户成功" : "设置窗户失败";
      return Json(EResult);
    }

    /// <summary>
    /// 设置窗帘
    /// </summary>
    /// <param name="controlParams"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SetCurtain(ControlParams controlParams)
    {
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Curtain"));
      byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x04 : controlParams.onoff == StateType.STOP ? 0x05 : 0x00);
      EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      EResult.Message = EResult.Status ? "设置窗帘成功" : "设置窗帘失败";
      return Json(EResult);
    }

    /// <summary>
    /// 设置空调参数
    /// </summary>
    /// <param name="airControlParams"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SetAirConditioning(AirControlParams airControlParams)
    {
      byte height = (byte)(airControlParams.onoff == StateType.OPEN ? 1 << 7 : 0 << 7);
      Int16 m = Convert.ToInt16(airControlParams.model);
      height |= (byte)(m << 4);
      Int16 s = Convert.ToInt16(airControlParams.speed);
      height |= (byte)(s << 2);
      height |= (byte)(Convert.ToInt16(airControlParams.SweepWind) << 1);
      byte low = (byte)Convert.ToInt16(airControlParams.wd);
      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("Air"));
      EResult = PortService.SendConvertCmd(fun, airControlParams.classroom, airControlParams.nodeAdd, height, low);
      EResult.Message = EResult.Status ? "设置空调成功" : "设置空调失败";
      return Json(EResult);
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

        byte[] classAddr = classroom.StrToHexByte();
        byte[] nodeAddr = nodeAdd.StrToHexByte();
        //获取当前时间
        DateTime currentTime = Convert.ToDateTime(DatetimeExtened.GetNetDateTime());
        //转换时间格式
        string year = (currentTime.Year % 100).ToString();
        string month = currentTime.Month < 10 ? "0" + currentTime.Month : currentTime.Month.ToString();
        string day = currentTime.Day < 10 ? "0" + currentTime.Day : currentTime.Day.ToString();
        string hour = currentTime.Hour < 10 ? "0" + currentTime.Hour : currentTime.Hour.ToString();
        string minute = currentTime.Minute < 10 ? "0" + currentTime.Minute : currentTime.Minute.ToString();
        string second = currentTime.Second < 10 ? "0" + currentTime.Second : currentTime.Second.ToString();
        string date = $"{year} {month} {day}";      //日期部分
        byte[] yMd = date.StrToHexByte();   //将日期部分转为byte[]类型
        string time = $"{hour} {minute} {second}";  //时间部分
        byte[] hms = time.StrToHexByte();   //将时间部分转为byte[]类型
        byte week = (byte)(Convert.ToInt32(currentTime.DayOfWeek.ToString("d")));

        byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x0D, 0, 0, 0, week, 0, 0, 0, 0x23, 0, 0, 0x70, 0, 0 };
        classAddr.CopyTo(cmd, 2);
        nodeAddr.CopyTo(cmd, 5);
        yMd.CopyTo(cmd, 7);
        hms.CopyTo(cmd, 11);
        cmd = cmd.ActuatorCommand();
        PortService.SendCmd(cmd);
        oa.Status = true;
        oa.Message = "时间同步成功";
        oa.ResultCode = ResultCode.Ok;
      }
      catch (Exception exception)
      {
        ExceptionHelper.AddException(exception);
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
    /// <param name="controlParams"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SetProjectionScreen(ControlParams controlParams)
    {

      byte fun = (byte)Convert.ToInt32(AppSettingUtils.GetValue("ProjectionScreen"));
      byte b = (byte)(controlParams.onoff == StateType.OPEN ? 0x01 : 0x00);
      EResult = PortService.SendConvertCmd(fun, controlParams.classroom, controlParams.nodeAdd, b);
      EResult.Message = EResult.Status ? "设置投影屏成功" : "设置投影屏失败";
      return Json(EResult);
    }

    /// <summary>
    /// 初始化设备节点
    /// </summary>
    /// <param name="classroom">教室地址</param>
    /// <returns></returns>
    public ActionResult Init(string classroom)
    {
      //cg5aU5K1iU
      byte b = 0x00;
      byte fun = 0x1f;
      EResult = PortService.SendConvertCmd(fun, classroom, "00", b);
      EResult.Message = EResult.Status ? "查询设备信息成功" : "查询设备信息失败";
      ClassRoom classRoom = PortService.GetReturnData(classroom);
      var list = classRoom.SonserList;
      List<Z_Equipment> zeList = new List<Z_Equipment>();
      Z_Room room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom).FirstOrDefault();
      ZEquipmentService.DeleteEntitys(ZEquipmentService.GetEntity(u => u.F_RoomId == room.F_Id).ToList());
      if (room != null)
      {
        foreach (var item in list)
        {
          var zEquipment = new Z_Equipment
          {
            F_Id = Guid.NewGuid().ToString(),
            F_RoomId = room.F_Id,
            F_FullName = item.Name,
            F_EquipmentType = item.Type + "",
            F_EnabledMark = true,
            F_EquipmentNo = item.Id.IndexOf('_') != -1 ? item.Id.Split('_')[0] : item.Id
          };
          zeList.Add(zEquipment);
        }
      }
      ZEquipmentService.AddEntitys(zeList);
      EResult.AppendData = list;
      //return Json(Result, JsonRequestBehavior.AllowGet);
      return Json(EResult, JsonRequestBehavior.AllowGet);
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
        //查找该楼栋
        Z_Room room = ZRoomService.GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();
        //该楼栋所有楼层
        var floors = ZRoomService.GetEntity(u => u.F_ParentId == room.F_Id);
        //楼层中所有教室
        var classroom = ZRoomService.GetEntity(u => floors.Any(f => f.F_Id == u.F_ParentId));
        //查找出所有教室里的所有该设备编码的设备
        var equis = ZEquipmentService.GetEntity(e => classroom.Any(r => e.F_RoomId == r.F_Id)).Where(e => e.F_EquipmentType == equipmentType);
        //筛选出教室编码和设备编码
        var val = (from c in classroom
                   join e in equis on c.F_Id equals e.F_RoomId
                   select new
                   {
                     c.F_RoomNo,
                     e.F_EquipmentNo
                   }).ToList();
        var listByte = new List<byte>();
        foreach (var item in val)
        {
          byte fun = (byte)GetFunByEquipmentType(equipmentType);
          byte b;
          if (equipmentType == EquipmentType.LAMP)    //控制灯的功能码
          {
            //该灯节点ID有多个灯设备
            if (val.Count(u => u.F_EquipmentNo == item.F_EquipmentNo) > 1)
            {
              b = (byte)(onoff == StateType.OPEN ? 0x03 : 0x00);
            }
            else    //该灯节点ID只有单个灯设备
            {
              b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
            }
          }
          else   //其它传感器的功能码
          {
            b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
          }
          byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, b };
          byte[] bclassroom = item.F_RoomNo.StrToHexByte();
          byte[] bnodeAdd = item.F_EquipmentNo.StrToHexByte();
          bclassroom.CopyTo(cmd, 2);
          bnodeAdd.CopyTo(cmd, 5);
          cmd = cmd.ActuatorCommand();
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
        byte b;
        if (equipmentType == EquipmentType.LAMP)//控制灯的功能码 灯比较特别
        {
          //该灯节点ID有多个灯设备
          if (val.Count(u => u.F_EquipmentNo == item.F_EquipmentNo) > 1)
          {
            b = (byte)(onoff == StateType.OPEN ? 0x03 : 0x00);
          }
          else//该灯节点ID只有单个灯设备
          {
            b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
          }
        }
        else //其它传感器的功能码
        {
          b = (byte)(onoff == StateType.OPEN ? 0x01 : 0x00);
        }
        byte[] cmd = { 0x55, 0x02, 0, 0, fun, 0, 0x01, b };
        byte[] bclassroom = item.F_RoomNo.StrToHexByte();
        byte[] bnodeAdd = item.F_EquipmentNo.StrToHexByte();
        bclassroom.CopyTo(cmd, 2);
        bnodeAdd.CopyTo(cmd, 5);
        cmd = cmd.ActuatorCommand();
        listByte.AddRange(cmd);
      }
      PortService.SendCmd(listByte.ToArray());
      return Json(val);
    }

    /// <summary>
    /// 通过楼栋查询所有教室异常设备信息
    /// </summary>
    /// <param name="buildingName">楼栋名称</param>
    /// <param name="layerName">楼层名称</param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SearchBuildingAllRoomEquipmentInfo(string buildingName, string layerName)
    {
      try
      {
        List<Buildings> list = Cache.GetCache<List<Buildings>>("allClassEquipmentInfo");
        foreach (var item in list)
        {
          EResult.ExceptionCount += item.ExceptionCount;
        }
        if (string.IsNullOrEmpty(buildingName) && string.IsNullOrEmpty(layerName))   //两个都为null 表示查询所有楼栋的教室设备信息
        {
          EResult.AppendData = list;
          EResult.Status = true;
          EResult.Message = "查询成功";
          EResult.ResultCode = ResultCode.Ok;
          return Json(EResult);
        }
        if (string.IsNullOrEmpty(layerName))    //楼层为null，表示查询整个楼栋教室设备的信息
        {
          Buildings building = list.FirstOrDefault(u => u.Name == buildingName);
          EResult.AppendData = building;
          EResult.Status = true;
          EResult.Message = "查询成功";
          EResult.ResultCode = ResultCode.Ok;
          return Json(EResult);
        }
        else                                    //楼栋，楼层都不为null，表示查询楼栋中，某层的教室设备信息
        {
          Buildings building = list.FirstOrDefault(u => u.Name == buildingName);
          if (building != null)
          {
            building.Floors = building.Floors.Where(u => u.Name == layerName).ToList();
            EResult.AppendData = building;
          }
          EResult.Message = "查询成功";
          EResult.Status = true;
          EResult.ResultCode = ResultCode.Ok;
          return Json(EResult);
        }
      }
      catch (Exception exception)
      {
        EResult.ErrorData = exception.Message;
        EResult.Message = "查询失败，请重试";
        EResult.ResultCode = ResultCode.Error;
        EResult.Status = false;
        return Json(EResult);
      }
    }

    /// <summary>
    /// 查询有设备异常的所有教室
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SearchBuildingAllRoomAbnormalEquipmentInfo()
    {
      try
      {
        List<Buildings> list = Cache.GetCache<List<Buildings>>("allClassEquipmentInfo");
        List<Buildings> _list = new List<Buildings>();
        foreach (var building in list)
        {
          building.Floors = building.Floors.Where(u => u.AbnormalEquipment).ToList();
          _list.Add(building);
        }
        EResult.AppendData = _list;
        EResult.Status = true;
        EResult.Message = "查询成功";
        EResult.ResultCode = ResultCode.Ok;

      }
      catch (Exception exception)
      {
        EResult.AppendData = exception.Message;
        EResult.Status = true;
        EResult.Message = "查询失败";
        EResult.ResultCode = ResultCode.Ok;
      }
      return Json(EResult);
    }

    /// <summary>
    /// 查询设备都正常的所有教室
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public ActionResult SearchBuildingAllRoomNormalEquipmentInfo()
    {
      try
      {
        List<Buildings> list = Cache.GetCache<List<Buildings>>("allClassEquipmentInfo");
        List<Buildings> _list = new List<Buildings>();
        foreach (var building in list)
        {
          building.Floors = building.Floors.Where(u => u.AbnormalEquipment == false).ToList();
          _list.Add(building);
        }
        EResult.AppendData = _list;
        EResult.Status = true;
        EResult.Message = "查询成功";
        EResult.ResultCode = ResultCode.Ok;

      }
      catch (Exception exception)
      {
        EResult.AppendData = exception.Message;
        EResult.Status = true;
        EResult.Message = "查询失败";
        EResult.ResultCode = ResultCode.Ok;
      }
      return Json(EResult);
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
        case EquipmentType.LAMP:
          fun = settings["Lamp"];
          break;
        case EquipmentType.DOOR:
          fun = settings["Door"];
          break;
        case EquipmentType.CURTAIN:
          fun = settings["Curtain"];
          break;
        case EquipmentType.WINDOW:
          fun = settings["Window"];
          break;
        case EquipmentType.AIR:
          fun = settings["Air"];
          break;
      }
      //Convert.ToInt32(fun);
      return Convert.ToInt32(fun);
    }
  }
}
