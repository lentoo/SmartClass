using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using IDAL;
using System.Linq.Expressions;

namespace DAL
{
    public partial class Sys_UserLogOnDal : BaseDal<Sys_UserLogOn>, ISys_UserLogOnDal
    {
        /// <summary>
        /// 根据用户ID查找用户密码相关信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Sys_UserLogOn GetEntityByUserId(string userId)
        {
            return dbContext.Set<Model.Sys_UserLogOn>().Where(u => u.F_UserId == userId).FirstOrDefault();
        }
    }
}
