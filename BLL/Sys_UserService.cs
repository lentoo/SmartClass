using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBLL;
using IDAL;

namespace BLL
{
    public partial class Sys_UserService : BaseService<Sys_User>, ISys_UserService
    {
        //public Sys_User GetEntityByAccount(string Account)
        //{
        //    return dal.GetEntitys(u => u.F_Account == Account).FirstOrDefault();
        //}
    }
}
