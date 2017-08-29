using SmartClass.IService;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Service
{
    public partial class Sys_LogService : BaseService<Sys_Log>, ISys_LogService
    {
        public bool AddLog(Sys_Log log)
        {
           return dal.AddEntity(log);
        }
    }
}
