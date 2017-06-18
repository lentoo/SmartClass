using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IBaseService<T> where T : class ,new()
    {
        IBaseDal<T> dal { get; set; }

        bool UpdateEntityInfo(T entity);
    }
}
