﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartClass.Models.Actuators
{
    public class Digital:SonserBase
    {
        /// <summary>
        /// 模拟量数值
        /// </summary>
        public string value { get; set; }
        public string state { get; set; }
    }
}