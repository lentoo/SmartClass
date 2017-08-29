using SmartClass.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DALFactory
{
    public class DbSessionFactory
    {
        public static IDbSession GetDbSession()
        {
            IDbSession currentDbSession = CallContext.GetData("DbSession") as DbSession;
            if (currentDbSession == null)
            {
                currentDbSession = new DbSession();
                CallContext.SetData("DbSession", currentDbSession);
            }
            return currentDbSession;
        }
    }
}
