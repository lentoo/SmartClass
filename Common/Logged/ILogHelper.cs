using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ILogHelper
    {
        void Debug(object ex);

        void Warn(object ex);

        void Error(object ex);

        void Info(object ex);
    }
}
