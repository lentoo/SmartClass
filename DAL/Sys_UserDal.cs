using DALFactory;
using SmartClass.IRepository;
using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Repository
{
    public partial class Sys_UserDal:BaseDal<Sys_User>,ISys_UserDal
    {

        /// <summary>
        /// 根据账号查找用户信息
        /// </summary>
        /// <param name="Account">账号</param>
        /// <returns></returns>
        public Sys_User GetEntityByAccount(string Account)
        {
            Sys_User user = dbContext.Set<Sys_User>().Where(u => u.F_Account == Account).FirstOrDefault();
            return user;
        }
    }

}
