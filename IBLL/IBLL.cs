 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;

namespace IBLL
{
	
	public partial interface ISys_LogService : IBaseService<Sys_Log>
    {
       
    }   
	
	public partial interface ISys_UserService : IBaseService<Sys_User>
    {
       
    }   
	
	public partial interface ISys_UserLogOnService : IBaseService<Sys_UserLogOn>
    {
       
    }   
	
	public partial interface IZ_EquipmentService : IBaseService<Z_Equipment>
    {
       
    }   
	
	public partial interface IZ_EquipmentLogService : IBaseService<Z_EquipmentLog>
    {
       
    }   
	
	public partial interface IZ_RoomService : IBaseService<Z_Room>
    {
       
    }   
	
}