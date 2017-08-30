using SmartClass.Infrastructure.Cache;
using SmartClass.IService;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartClass.Infrastructure.Exception;

namespace SmartClass.Service
{
  public partial class Z_SectionTimeService
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
        if (list == null)
        {
          list = GetEntity(u => true).ToList();
          CacheHelper.AddCache("SectionTime", list, DateTime.Now.AddDays(7));
        }
        return list;
      }
      catch (Exception ex)
      {
        ExceptionHelper.AddException(ex);
        return null;
      }
    }
  }
}
