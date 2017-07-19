using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Cache;
using IBLL;
using Model.Properties;
using SmartClass.Models.Classes;

namespace SmartClass.Controllers
{
    public class ClassesController : Controller
    {
        public IZ_RoomService ZRoomService { get; set; }
        /// <summary>
        /// 查询所有的教室
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchAllClass()
        {
            List<Buildings> list = CacheHelper.GetCache<List<Buildings>>("AllClasses");
            if (list != null)
            {
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            list = new List<Buildings>();
            var Buildings = ZRoomService.GetEntity(u => u.F_RoomType == "Building").ToList();
            var Floors = ZRoomService.GetEntity(u => u.F_RoomType == "Floor").ToList();
            var ClassRooms = ZRoomService.GetEntity(u => u.F_RoomType == "ClassRoom").ToList();

            foreach (var item in Buildings)
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
                        classRoom.CollegeName = item.F_FullName;
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
                CacheHelper.AddCache("AllClasses", list, DateTime.Now.AddDays(1));
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}