using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ISys_UserDal<T>:IBaseDal<T> where T:class ,new()
    {
        /// <summary>
        /// 根据账号查找用户信息
        /// </summary>
        /// <param name="Account">账号</param>
        /// <returns></returns>
        Sys_User GetEntityByAccount(string Account);
    }
}
