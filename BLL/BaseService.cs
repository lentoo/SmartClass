using DAL;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BaseService<T>:IBaseService<T> where T : class, new()
    {
        public IBaseDal<T> dal = new BaseDal<T>();

        public bool UpdateEntityInfo(T entity)
        {
           return dal.UpdateEntityInfo(entity);
        }
    }
}
