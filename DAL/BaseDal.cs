using IDAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class BaseDal<T>:IBaseDal<T> where T : class ,new()
    {
       
        public DbContext Db
        {
            get
            {
                return DbContextFactory.GetDbContext();
            }
            set
            {
                DbContextFactory.GetDbContext();
            }
        }

        public bool AddEntity(T entity)
        {
            Db.Set<T>().Add(entity);
            if (Db.SaveChanges() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region 查询
        public IQueryable<T> GetEntitys(Expression<Func<T, bool>> whereLambda)
        {
            return Db.Set<T>().Where(whereLambda);
        }

        /// <summary>
        /// 修改登录日志信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateEntityInfo(T entity)
        {
            Db.Entry(entity).State =EntityState.Modified;
            if (Db.SaveChanges() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
