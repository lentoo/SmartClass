using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public partial interface IZ_SectionTimeService
    {
        /// <summary>
        /// 获取节次上课时间
        /// </summary>
        /// <returns></returns>
        List<Z_SectionTime> GetSectionTime();
    }
}
