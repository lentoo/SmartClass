using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartClass.Models.Types
{
    public class StateType
    {
        /// <summary>
        /// 打开
        /// </summary>
        public static readonly string OPEN = "open";
        /// <summary>
        /// 关闭
        /// </summary>
        public static readonly string CLOSE = "close";
        /// <summary>
        /// 停止
        /// </summary>
        public static readonly string STOP = "stop";
        /// <summary>
        /// 在线
        /// </summary>
        public static readonly string Online = "OnLine";
        /// <summary>
        /// 离线
        /// </summary>
        public static readonly string Offline = "OffLine";

        public static readonly string StateOpen = "打开";
        public static readonly string StateClose = "关闭";
    }
}