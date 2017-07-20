using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Model;
using System.Threading.Tasks;

namespace DAL
{
    public class DbContextFactory
    {
        public static DbContext GetDbContext()
        {
            DbContext context = CallContext.GetData("dbContext") as NFineBaseEntities;
            
            if (context == null)
            {
                context = new NFineBaseEntities();               
                CallContext.SetData("dbContext", context);
            }
            return context;
        }
    }
}
