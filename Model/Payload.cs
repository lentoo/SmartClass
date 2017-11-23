using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
  /// <summary>
  /// 信息载体
  /// </summary>
  public class Payload
  {
    public string Account { get; set; }
    public DateTime Exp { get; set; }
    public string IMEI { get; set; }
    public string Issuer { get; set; }
  }
}
