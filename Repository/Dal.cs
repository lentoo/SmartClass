 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartClass.IRepository;
using Model;

namespace SmartClass.Repository
{
		
	public partial class Sys_LogDal :BaseDal<Sys_Log>,ISys_LogDal
    {

    }
		
	public partial class Sys_UserDal :BaseDal<Sys_User>,ISys_UserDal
    {

    }
		
	public partial class Sys_UserLogOnDal :BaseDal<Sys_UserLogOn>,ISys_UserLogOnDal
    {

    }
		
	public partial class Z_AttendanceDal :BaseDal<Z_Attendance>,IZ_AttendanceDal
    {

    }
		
	public partial class Z_AttendanceDetailsDal :BaseDal<Z_AttendanceDetails>,IZ_AttendanceDetailsDal
    {

    }
		
	public partial class Z_ClassDal :BaseDal<Z_Class>,IZ_ClassDal
    {

    }
		
	public partial class Z_ClassComputeDal :BaseDal<Z_ClassCompute>,IZ_ClassComputeDal
    {

    }
		
	public partial class Z_CourseDal :BaseDal<Z_Course>,IZ_CourseDal
    {

    }
		
	public partial class Z_DepartmentDal :BaseDal<Z_Department>,IZ_DepartmentDal
    {

    }
		
	public partial class Z_EquipmentDal :BaseDal<Z_Equipment>,IZ_EquipmentDal
    {

    }
		
	public partial class Z_EquipmentLogDal :BaseDal<Z_EquipmentLog>,IZ_EquipmentLogDal
    {

    }
		
	public partial class Z_GradeDal :BaseDal<Z_Grade>,IZ_GradeDal
    {

    }
		
	public partial class Z_ProfessionDal :BaseDal<Z_Profession>,IZ_ProfessionDal
    {

    }
		
	public partial class Z_RoomDal :BaseDal<Z_Room>,IZ_RoomDal
    {

    }
		
	public partial class Z_SchoolTimeDal :BaseDal<Z_SchoolTime>,IZ_SchoolTimeDal
    {

    }
		
	public partial class Z_SectionTimeDal :BaseDal<Z_SectionTime>,IZ_SectionTimeDal
    {

    }
		
	public partial class Z_StudentDal :BaseDal<Z_Student>,IZ_StudentDal
    {

    }
	

}