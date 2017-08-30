using AutoMapper;
using Model.DTO.Classes;
using Model.DTO.Courses;

namespace Model.AutoMapperConfig
{
  public class AutoMapperConfig
  {
    /// <summary>
    /// 初始化映射关系
    /// </summary>
    public static void InitMapping()
    {
      Mapper.Initialize(cfg =>
      {
        #region Z_Course => Course
        var courseMap = cfg.CreateMap<Z_Course, Course>();
        courseMap.ConstructUsing(course => new Course()
        {
          Id = course.F_Id,
          TeacherName = course.F_TeacherName,
          Week = course.F_Week,
          CourseName = course.F_FullName,
          Grade = course.F_Grade,
          Major = course.F_Major,
          Classes = course.F_Class,
          RoomName = course.F_RoomName,
          RoomNo = course.F_RoomCode,
          CourseTimeType = course.F_CourseTimeType,
          EnCode = course.F_EnCode,
          BeginWeek = course.F_BeginWeek,
          EndWeek = course.F_EndWeek
        });
        //courseMap.ForMember(zc => zc.Id, op => op.MapFrom(a => a.F_Id));
        //courseMap.ForMember(zc => zc.Grade, op => op.MapFrom(a => a.F_Grade));
        //courseMap.ForMember(zc => zc.Classes, op => op.MapFrom(a => a.F_Class));
        //courseMap.ForMember(zc => zc.CourseName, op => op.MapFrom(a => a.F_FullName));
        //courseMap.ForMember(zc => zc.BeginWeek, op => op.MapFrom(a => a.F_BeginWeek));
        //courseMap.ForMember(zc => zc.EndWeek, op => op.MapFrom(a => a.F_EndWeek));
        //courseMap.ForMember(zc => zc.TeacherName, op => op.MapFrom(a => a.F_TeacherName));
        //courseMap.ForMember(zc => zc.CourseTimeType, op => op.MapFrom(a => a.F_CourseTimeType));
        //courseMap.ForMember(zc => zc.EnCode, op => op.MapFrom(a => a.F_EnCode));
        //courseMap.ForMember(zc => zc.RoomNo, op => op.MapFrom(a => a.F_RoomCode));
        //courseMap.ForMember(zc => zc.Major, op => op.MapFrom(a => a.F_Major));
        //courseMap.ForMember(zc => zc.RoomName, op => op.MapFrom(a => a.F_RoomName));
        //courseMap.ForMember(zc => zc.Week, op => op.MapFrom(a => a.F_Week)); 
        #endregion

        #region Z_Room,Floors,Buildings  => ClassRoom
        var classrooMap = cfg.CreateMap<Z_Room, ClassRoom>();
        classrooMap.ForMember(cls => cls.Name, ops => ops.MapFrom(r => r.F_FullName))
          .ForMember(cls => cls.Id, ops => ops.MapFrom(r => r.F_RoomNo))
          .ForMember(cls => cls.ClassNo, ops => ops.MapFrom(r => r.F_EnCode));
        cfg.CreateMap<Floors, ClassRoom>().ForMember(cls => cls.LayerName, ops => ops.MapFrom(f => f.Name));
        cfg.CreateMap<Buildings, ClassRoom>().ForMember(cls => cls.BuildingName, ops => ops.MapFrom(b => b.Name));
      });
      #endregion

    }

    public static T Map<T>(object obj)
    {
      return Mapper.Map<T>(obj);
    }
    /// <summary>
    /// 合并两个对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="obj"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static T1 Map<T, T1>(T obj, T1 obj2)
    {
      return Mapper.Map(obj, obj2);
    }
  }
}