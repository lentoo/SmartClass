using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace SmartClass.IService
{
    public partial interface IZ_EquipmentService
    {
        /// <summary>
        /// 检查教室是否有该设备
        /// </summary>
        /// <param name="classroom">教室地址</param>
        /// <param name="nodeAdd">节点地址</param>
        /// <returns></returns>
        bool CheckClassEquipment(string classroom, string nodeAdd);
    }
}
