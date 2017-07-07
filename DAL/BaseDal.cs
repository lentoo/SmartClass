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

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public bool ExceptionSql(string sql,object[]paramters)
        {
           int i= Db.Database.ExecuteSqlCommand(sql, paramters);
            
            if (i > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddEntitys(IEnumerable<T> entitys)
        {
            Db.Set<T>().AddRange(entitys);
            return Db.SaveChanges() > 0;
        }
    }
}
