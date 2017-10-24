using Model.Actuators;
using SmartClass.Infrastructure.Cache;
using SmartClass.Models.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartClass.Models.SerialPortRelated
{

  /// <summary>
  /// 串口数据处理抽象类
  /// </summary>
  public abstract class SerialPortDataProcess
  {
    public List<SensorBase> Sensors { get; set; }
    public ICacheHelper Cache { get; set; }
    public string classRoomId { get; set; }
    public SerialPortDataProcess(ICacheHelper Cache,string classRoomId)
    {
      this.Cache = Cache;
      this.classRoomId = classRoomId;
    }
    public SerialPortDataProcess(){}
    /// <summary>
    /// 串口数据处理
    /// </summary>
    /// <returns></returns>
    public abstract int DataProcessing(int index, byte[] data, int type, string name);

    /// <summary>
    /// 获取灯传感器在线状态
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public string OnLineState(int state)
    {
      return state >> 4 == 0 ? StateType.Offline : StateType.Online;
    }
  }
}