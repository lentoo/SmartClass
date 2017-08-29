using SmartClass.IService;
using SmartClass.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SmartClass.Service
{
    public class BaseService<T> : IBaseService<T> where T : class, new()
    {
        public IBaseDal<T> dal { get; set; }

        public bool DeleteEntity(T entity)
        {
            return dal.DeleteEntity(entity);
        }
        public bool DeleteEntitys(IEnumerable<T> entitys)
        {
            return dal.DeleteEntitys(entitys);
        }

        public bool AddEntity(T entity)
        {
            return dal.AddEntity(entity);
        }

        public bool AddEntitys(IEnumerable<T> entity)
        {
            return dal.AddEntitys(entity);
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
