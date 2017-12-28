using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Model.Enum;

namespace SmartClass.Infrastructure.Extended
{
  /// <summary>
  /// 对string类型方法扩展
  /// </summary>
  public static class StringExtended
  {
    /// <summary>
    /// 字符串转16进制字节数组
    /// </summary>
    /// <param name="str">要转换的字符串</param>
    /// <returns>16进制字节数组</returns>
    public static byte[] StrToHexByte(this string str)
    {
      str = str.Length == 1 ? 0 + str : str;
      str = str.Replace(" ", "");
      if ((str.Length % 2) != 0)
        str += " ";
      byte[] returnBytes = new byte[str.Length / 2];
      for (int i = 0; i < returnBytes.Length; i++)
        returnBytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
      return returnBytes;
    }
    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="str">加密字符</param>
    /// <param name="code">加密位数16/32</param>
    /// <returns></returns>
    public static string ToMd5(this string str)
    {
      MD5 md5 = MD5.Create();
      byte[] buff = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
      StringBuilder sb = new StringBuilder();
      foreach (var b in buff)
      {
        sb.Append(b.ToString("x2"));
      }
      md5.Clear();
      return sb.ToString();
    }

    /// <summary>
    /// 通过设备类型获取功能码
    /// </summary>
    /// <param name="equipmentType">设备类型</param>
    /// <returns></returns>
    public static int GetFunByEquipmentType(this string equipmentType)
    {
      string fun = string.Empty;
      var settings = ConfigurationManager.AppSettings;
      switch (equipmentType)
      {
        case EquipmentType.LAMP:
          fun = settings["Lamp"];
          break;
        case EquipmentType.DOOR:
          fun = settings["Door"];
          break;
        case EquipmentType.CURTAIN:
          fun = settings["Curtain"];
          break;
        case EquipmentType.WINDOW:
          fun = settings["Window"];
          break;
        case EquipmentType.AIR:
          fun = settings["Air"];
          break;
      }
      //Convert.ToInt32(fun);
      return Convert.ToInt32(fun);
    }
  }
  /// <summary>
  /// 对byte[]类型方法扩展
  /// </summary>
  public static class BytesExtended
  {
    /// <summary>
    /// 通过命令获取CRC16转换后的命令
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static byte[] ActuatorCommand(this byte[] cmd)
    {
      //获取CRC校验码
      byte[] crcCode = cmd.Crc();
      byte[] data = new byte[cmd.Length + 3];
      cmd.CopyTo(data, 0);
      data[data.Length - 1] = 0xbb;
      data[data.Length - 3] = crcCode[0];
      data[data.Length - 2] = crcCode[1];
      return data;
    }
    public static byte[] Crc(this byte[] buff)
    {
      UInt16 reg = 0xffff;
      foreach (var b in buff)
      {
        reg ^= b;

        for (int i = 0; i < 8; i++)
        {
          int flag = 1;
          flag = flag & reg;
          reg >>= 1;
          if (flag == 1)
          {
            reg ^= 0xA001;
          }
        }
      }

      return new[] { (byte)(reg >> 8), (byte)(reg) };
    }
    public static string HexToStr(this byte[]buff)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in buff)
        {
            stringBuilder.Append(item.ToString("X2")+" ");
        }
        return stringBuilder.ToString();
    }
  }
}
