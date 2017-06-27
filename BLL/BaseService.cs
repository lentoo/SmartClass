using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BaseService<T> : IBaseService<T> where T : class, new()
    {
        //public IBaseDal<T> dal = new BaseDal<T>();
        public IBaseDal<T> dal { get; set; }

        public bool AddEntity(T entity)
        {
            return dal.AddEntity(entity);
        }

        public bool UpdateEntityInfo(T entity)
        {
            return dal.UpdateEntityInfo(entity);
        }
    }
}
