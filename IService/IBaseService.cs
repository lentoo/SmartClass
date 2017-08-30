using SmartClass.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.IService
{
    public interface IBaseService<T> where T : class ,new()
    {
        IBaseDal<T> dal { get; set; }
        
        /// <summary>
        ///  修改实体信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool UpdateEntityInfo(T entity);
        bool DeleteEntity(T entity);
        bool DeleteEntitys(IEnumerable<T> entitys);
        /// <summary>
        /// 添加实体信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AddEntity(T entity);
        bool AddEntitys(IEnumerable<T> entity);
        /// <summary>
        /// 根据条件得到实体信息
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        IQueryable<T> GetEntity(Expression<Func<T, bool>> whereLambda);
    }
}
