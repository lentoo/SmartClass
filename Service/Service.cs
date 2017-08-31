 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using SmartClass.IService;

namespace SmartClass.Service
{
	
	public partial class Sys_LogService :BaseService<Sys_Log>,ISys_LogService
    {
    }   
	
	public partial class Sys_UserService :BaseService<Sys_User>,ISys_UserService
    {
    }   
	
	public partial class Sys_UserLogOnService :BaseService<Sys_UserLogOn>,ISys_UserLogOnService
    {
    }   
	
	public partial class Z_AttendanceService :BaseService<Z_Attendance>,IZ_AttendanceService
    {
    }   
	
	public partial class Z_AttendanceDetailsService :BaseService<Z_AttendanceDetails>,IZ_AttendanceDetailsService
    {
    }   
	
	public partial class Z_ClassService :BaseService<Z_Class>,IZ_ClassService
    {
    }   
	
	public partial class Z_CourseService :BaseService<Z_Course>,IZ_CourseService
    {
    }   
	
	public partial class Z_DepartmentService :BaseService<Z_Department>,IZ_DepartmentService
    {
    }   
	
	public partial class Z_EquipmentService :BaseService<Z_Equipment>,IZ_EquipmentService
    {
    }   
	
	public partial class Z_EquipmentLogService :BaseService<Z_EquipmentLog>,IZ_EquipmentLogService
    {
    }   
	
	public partial class Z_GradeService :BaseService<Z_Grade>,IZ_GradeService
    {
    }   
	
	public partial class Z_ProfessionService :BaseService<Z_Profession>,IZ_ProfessionService
    {
    }   
	
	public partial class Z_RoomService :BaseService<Z_Room>,IZ_RoomService
    {
    }   
	
	public partial class Z_SchoolTimeService :BaseService<Z_SchoolTime>,IZ_SchoolTimeService
    {
    }   
	
	public partial class Z_SectionTimeService :BaseService<Z_SectionTime>,IZ_SectionTimeService
    {
    }   
	
	public partial class Z_StudentService :BaseService<Z_Student>,IZ_StudentService
    {
    }   
	
}