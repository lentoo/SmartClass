 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDAL;
using Model;

namespace DAL
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
		
	public partial class Z_CourseDal :BaseDal<Z_Course>,IZ_CourseDal
    {

    }
		
	public partial class Z_EquipmentDal :BaseDal<Z_Equipment>,IZ_EquipmentDal
    {

    }
		
	public partial class Z_EquipmentLogDal :BaseDal<Z_EquipmentLog>,IZ_EquipmentLogDal
    {

    }
		
	public partial class Z_RoomDal :BaseDal<Z_Room>,IZ_RoomDal
    {

    }
	

}