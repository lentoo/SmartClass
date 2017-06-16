using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutofacDemo
{
    public class UserService : IUserService
    {
        public string Show(string name)
        {
            return name;
        }
    }
}
