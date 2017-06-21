using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using IBLL;
using IDAL;

namespace BLL
{
    public class Sys_UserLogOnService : BaseService<Sys_UserLogOn>, ISys_UserLogOnService
    {
          public ISys_UserLogOnDal UserLogOnDal { get; set; }

        public Sys_UserLogOn GetEntityByUserId(string userId)
        {
            return dal.GetEntitys(u => u.F_UserId == userId).FirstOrDefault();
        }
    }
}
