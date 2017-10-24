using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO.Courses
{
  public class SchollTime
  {
    /// <summary>
    /// 学年
    /// </summary>
    public string SearchYear { get; set; }
    /// <summary>
    /// 年
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// 月
    /// </summary>
    public int Month { get; set; }
    /// <summary>
    /// 第几学期
    /// </summary>
    public int Term { get; set; }
    /// <summary>
    /// 当前第几周
    /// </summary>
    public int Weeks { get; set; }
    public DateTime CurrentTime { get; set; }
    /// <summary>
    /// 星期几
    /// </summary>
    public string Week { get; set; }
  }
}
