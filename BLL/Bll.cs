﻿ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using IBLL;

namespace BLL
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

    public partial class Z_CourseService : BaseService<Z_Course>, IZ_CourseService
    {       
    }

    public partial class Z_EquipmentService :BaseService<Z_Equipment>,IZ_EquipmentService
    {
    }   
	
	public partial class Z_EquipmentLogService :BaseService<Z_EquipmentLog>,IZ_EquipmentLogService
    {
    }   
	
	public partial class Z_RoomService :BaseService<Z_Room>,IZ_RoomService
    {
    }   
	
	public partial class Z_SchoolTimeService :BaseService<Z_SchoolTime>,IZ_SchoolTimeService
    {
    }

    public partial class Z_SectionTimeService : BaseService<Z_SectionTime>, IZ_SectionTimeService
    {       
    }

}