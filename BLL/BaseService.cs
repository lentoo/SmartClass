using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

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

        public IQueryable<T> GetEntity(Expression<Func<T, bool>> whereLambda)
        {
            return dal.GetEntitys(whereLambda);
        }

        public bool UpdateEntityInfo(T entity)
        {
            return dal.UpdateEntityInfo(entity);
        }
    }
}
