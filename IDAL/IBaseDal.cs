
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IBaseDal<T> where T:class,new()
    {
      //  DbContext Db { get; set; }
        /// <summary>
        /// 根据条件得到实体信息
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        IQueryable<T> GetEntitys(Expression<Func<T, bool>> whereLambda);

        bool UpdateEntityInfo(T entity);
    }
}
