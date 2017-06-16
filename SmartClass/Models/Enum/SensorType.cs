using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SmartClass.Models.Enum
{
    public enum SensorType
    {
        [Description("照明")]
        Illumination = 0,
        [Description("门")]
        Door = 1,
        [Description("窗帘")]
        Curtain = 2,
        [Description("窗户")]
        Window = 3,
        [Description("风机")]
        Fan = 4,
        [Description("人体")]
        Human = 5,
        [Description("气体")]
        Gas = 6,
        [Description("烟雾")]
        Smoke = 7,
        [Description("报警")]
        Alarm = 8,
        [Description("温度")]
        Temperature = 9,
        [Description("湿度")]
        Humidity = 10,
        [Description("光照")]
        Light = 11,
        [Description("PM2.5")]
        PM2_5 = 12,
        [Description("空调")]
        AirConditioning = 13,
        [Description("电子钟")]
        ElectronicClock = 13
    }
    public enum ActuatorType
    {
        On = 1,
        Off = 0,
    }
}