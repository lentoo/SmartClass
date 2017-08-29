
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.IRepository
{
    public interface IBaseDal<T> where T:class,new()
    {
        DbContext dbContext { get;  }
        /// <summary>
        /// 根据条件得到实体信息
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        IQueryable<T> GetEntitys(Expression<Func<T, bool>> whereLambda);

        bool UpdateEntityInfo(T entity);

        bool DeleteEntity(T entity);
        bool DeleteEntitys(IEnumerable<T> entitys);
        /// <summary>
        /// 添加一个实体信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AddEntity(T entity);

        bool AddEntitys(IEnumerable<T> entity);
    }
}
