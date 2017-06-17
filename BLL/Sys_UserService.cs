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
    public class Sys_UserService : BaseService<Sys_User>, ISys_UserService
    {
       // public ISys_UserDal UserDal { get; set; }
        /// <summary>
        /// 根据账号查找用户信息
        /// </summary>
        /// <param name="Account">账号</param>
        /// <returns></returns>
        public Sys_User GetEntityByAccount(string Account)
        {
            return dal.GetEntitys(u => u.F_Account == Account).FirstOrDefault();
        }
    }
}
