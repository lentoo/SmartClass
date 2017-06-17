using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace DAL
{
    public class Sys_UserLogOnDal : BaseDal<Sys_UserLogOnDal>, IDAL.ISys_UserLogOnDal<Sys_UserLogOnDal>
    {
        /// <summary>
        /// 根据用户ID查找用户密码相关信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Sys_UserLogOn GetEntityByUserId(string userId)
        {
            return Db.Set<Model.Sys_UserLogOn>().Where(u => u.F_UserId == userId).FirstOrDefault();
        }
    }
}
