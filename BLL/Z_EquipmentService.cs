using System;
using System.Linq;

namespace BLL
{
    public partial class Z_EquipmentService
    {
        public IBLL.IZ_RoomService ZRoomService { get; set; }
        public bool CheckClassEquipment(string classroom, string nodeAdd)
        {
            string node = Convert.ToInt32(nodeAdd) + "";
            var room = ZRoomService.GetEntity(u => u.F_RoomNo == classroom);
            var equipment = GetEntity(u => u.F_EquipmentNo == node);
            var roomEquipment =
            (from r in room
             join e in equipment on r.F_Id equals e.F_RoomId
             select new
             {
                 classroom = r.F_RoomNo,
                 className = r.F_FullName,
                 nodeAdd = e.F_EquipmentNo,
                 nodeName = e.F_FullName
             }).FirstOrDefault();
            if (roomEquipment != null)
            {
                return true;
            }
            return false;
        }
    }

}