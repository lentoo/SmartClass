using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Service
{
  public partial class Z_RoomService
  {
    /// <summary>
    /// 获取楼栋名称下的所有的房间
    /// </summary>
    /// <param name="buildingName">楼栋名称</param>
    /// <param name="floorName">楼层名称</param>
    /// <param name="roomName">教室名称</param>
    /// <returns></returns>
    public Z_Room[] GetRoomsForBuildingName(string buildingName, string floorName = null, string roomName = null)
    {
      //查找该楼栋
      Z_Room room = GetEntity(u => u.F_FullName == buildingName).FirstOrDefault();
      //该楼栋所有楼层
      var floors = GetEntity(u => u.F_ParentId == room.F_Id);

      if (null != floorName)
      {
        // 获取指定楼层下的所有房间
        floors = floors.Where(u => u.F_FullName == floorName);
      }

      //楼层中所有教室
      var classroom = GetEntity(u => floors.Any(f => f.F_Id == u.F_ParentId)).ToArray();

      classroom = roomName == null ? classroom : classroom.Where(c => c.F_FullName == roomName).ToArray();
      return classroom;
    }
  }
}
