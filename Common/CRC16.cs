using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class CRC16
    {
       public static byte[] Crc(byte[] buff)
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

            return new byte[] { (byte)(reg>>8), (byte)(reg ) };
        }
    }
}
