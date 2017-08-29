using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using SmartClass.Models.Classes;
using Models.Classes;
using SmartClass.Models;
using Model.Enum;
using Model;
using SmartClass.Infrastructure.Extended;

namespace SmartClass.Controllers
{
    /// <summary>
    /// 教室信息控制器
    /// </summary>
    public class ClassesController : Controller
    {
        public IZ_RoomService ZRoomService { get; set; }
        public ICacheHelper Cache { get; set; }
        /// <summary>
        /// 查询所有的教室
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchAllClass()
        {
            List<Buildings> list = Cache.GetCache<List<Buildings>>("AllClasses");
            if (list != null)
            {
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            list = new List<Buildings>();
            var buildings = ZRoomService.GetEntity(u => u.F_RoomType == "Building").ToList();
            var Floors = ZRoomService.GetEntity(u => u.F_RoomType == "Floor").ToList();
            var ClassRooms = ZRoomService.GetEntity(u => u.F_RoomType == "ClassRoom").ToList();

            foreach (var item in buildings)
            {
                Buildings building = new Buildings();
                building.Name = item.F_FullName;
                List<Floors> FList = new List<Floors>();
                foreach (var f in Floors)
                {
                    if (f.F_ParentId != item.F_Id) continue;
                    Floors floors = new Floors();
                    floors.Name = f.F_FullName;
                    List<ClassRoom> classRooms = new List<ClassRoom>();
                    foreach (var rooms in ClassRooms)
                    {
                        if (rooms.F_ParentId != f.F_Id) continue;

                        ClassRoom classRoom = new ClassRoom();
                        classRoom.Id = rooms.F_RoomNo;
                        classRoom.BuildingName = item.F_FullName;
                        classRoom.LayerName = f.F_FullName;
                        classRoom.Name = rooms.F_FullName;
                        classRoom.ClassNo = rooms.F_EnCode;
                        classRooms.Add(classRoom);
                    }
                    floors.ClassRooms = classRooms;
                    FList.Add(floors);
                }
                building.Floors = FList;
                list.Add(building);
            }
            Cache.AddCache("AllClasses", list, DateTime.Now.AddDays(1));
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 模糊查询教室信息，通过教室名称和编号
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ActionResult SearchClassListByNameOrNo(string name)
        {
            var room = ZRoomService.GetEntity(u => u.F_FullName.Contains(name) || u.F_EnCode.Contains(name)).Select(u => new
            {
                value = u.F_FullName + "(" + u.F_EnCode + ")",
                u.F_FullName,
                u.F_EnCode,
                u.F_RoomNo,
            }).ToList();
            return Json(room, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 报警信息
        /// </summary>
        /// <returns></returns>
        public ActionResult AlarmInformation()
        {
            if (SerialPortUtils.AlarmData.Count > 0) //有报警数据
            {
                byte[]data=SerialPortUtils.AlarmData.Dequeue();
                string classId = Convert.ToString(data[2],16) + Convert.ToString(data[3],16);
                Z_Room room= ZRoomService.GetEntity(r => r.F_RoomNo == classId).FirstOrDefault();
                int value = data[7];
                return Json(new { ResultCode = ResultCode.Error,ClassName=room.F_FullName,AppendData=value,ClassNo=room.F_EnCode});
            }
            return Json(new { ResultCode = ResultCode.Ok });
        }
    }
}