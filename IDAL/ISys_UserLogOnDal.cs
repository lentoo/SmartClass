using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
   public interface ISys_UserLogOnDal:IBaseDal<Sys_UserLogOn>
    {
        /// <summary>
        /// 根据用户ID查找用户密码相关信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Sys_UserLogOn GetEntityByUserId(string userId);
    }
}
