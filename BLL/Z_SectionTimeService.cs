using Common.Cache;
using IBLL;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public partial class Z_SectionTimeService : BaseService<Z_SectionTime>, IZ_SectionTimeService
    {
        public ICacheHelper CacheHelper { get; set; }
        /// <summary>
        /// 获取节次上课时间
        /// </summary>
        /// <returns></returns>
        public List<Z_SectionTime> GetSectionTime()
        {
            try
            {
                List<Z_SectionTime> list = CacheHelper.GetCache<List<Z_SectionTime>>("SectionTime");
                list = list ?? GetEntity(u => true).ToList();
                CacheHelper.AddCache("SectionTime", list);
                return list;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
