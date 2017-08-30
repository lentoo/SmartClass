using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
  /// <summary>
  /// 空调控制参数
  /// </summary>
  public class AirControlParams:ControlParams
  {
    /// <summary>
    /// 模式  0->自动,1->制冷,2->制热,3->送风
    /// </summary>
    public string model { get; set; }
    /// <summary>
    /// 风速  0->低,1->中,2->高
    /// </summary>
    public string speed { get; set; }
    /// <summary>
    /// 扫风  0->不扫风,1->扫风
    /// </summary>
    public string SweepWind { get; set; }
    /// <summary>
    /// 温度  0-15对应 16度到30度
    /// </summary>
    public string wd { get; set; }
  }
}
