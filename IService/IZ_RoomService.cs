using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.IService
{
  public partial interface IZ_RoomService
  {
    /// <summary>
    /// 获取楼栋名称下的所有的房间
    /// </summary>
    /// <param name="buildingName">楼栋名称</param>
    /// <param name="floorName">楼层名称</param>
    /// <param name="roomName">教室名称</param>
    /// <returns></returns>
    Z_Room[] GetRoomsForBuildingName(string buildingName, string floorName = null, string room = null);
  }
}
