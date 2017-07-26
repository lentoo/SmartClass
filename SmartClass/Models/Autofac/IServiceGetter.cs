using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClass.Models.Autofac
{
    public interface IServiceGetter
    {
        T GetByName<T>(string name);
    }
}
