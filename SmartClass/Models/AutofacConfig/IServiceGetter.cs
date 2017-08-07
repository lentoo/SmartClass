using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Models.AutofacConfig
{
    public interface IServiceGetter
    {
        T GetByName<T>(string name);
        T GetService<T>();
    }
}
