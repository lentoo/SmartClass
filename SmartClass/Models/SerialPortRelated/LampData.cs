using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartClass.Infrastructure.Cache;
using Model.Actuators;
using SmartClass.Models.Types;

namespace SmartClass.Models.SerialPortRelated
{
  public class LampData : SerialPortDataProcess
  {
    public LampData(ICacheHelper Cache,string classRoomId) : base(Cache, classRoomId)
    {
    }

    public override int DataProcessing(int index, byte[] data, int type, string name)
    {
      Actuator Lamp1 = new Actuator();
      string moduleId = Convert.ToString(data[index++], 16);
      Lamp1.Name = name;
      Lamp1.Type = type;
      int state = data[index++];
      int moduleNum = state >> 7;

      if (moduleNum == 1)         //一个节点控制两盏灯
      {
        //将单个节点控制多个灯的状态值保存起来
        Cache.AddCache(classRoomId, state);
        int moduleState = state & 0x1;
        Lamp1.Id = moduleId + "_0";
        Lamp1.State = moduleState != 0 ? StateType.StateOpen : StateType.StateClose;
        Lamp1.IsOpen = moduleState == 1;
        //数据位第4位表示在线状态
        Lamp1.Online = OnLineState(state);
        Lamp1.Controllable = true;
        Sensors.Add(Lamp1);
        Actuator Lamp2 = new Actuator();
        Lamp2.Name = name;
        Lamp2.Type = type;
        Lamp2.Id = moduleId + "_1";
        int module1State = (state >> 1) & 0x1;
        Lamp2.State = module1State != 0 ? StateType.StateOpen : StateType.StateClose;
        Lamp2.IsOpen = module1State == 1;
        Lamp2.Online = OnLineState(state);
        Lamp2.Controllable = true;
        Sensors.Add(Lamp2);
      }
      else
      {
        Lamp1.Id = moduleId;
        Lamp1.State = (state & 1) != 0 ? StateType.StateOpen : StateType.StateClose;
        Lamp1.IsOpen = (state & 1) == 1;
        Lamp1.Online = OnLineState(state);
        Lamp1.Controllable = true;
        Sensors.Add(Lamp1);
      }
      return index;
    }
  }
}