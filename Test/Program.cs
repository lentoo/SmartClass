using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //byte[] data = { 0x55, 0x02, 0x12, 0x34, 0x01 ,0x22, 0x01, 0x01 };
            // byte[]b = Common.CRC16.Crc(data);
            //Console.WriteLine(Convert.ToInt32(b[0])+"   "+b[1]);
            string str = "55";
            byte[]data=StrToHexByte(str);
             Console.ReadKey();
        }

        public static byte[] StrToHexByte(string str)
        {
            str = str.Replace(" ", "");
            if ((str.Length % 2) != 0)
                str += " ";
            byte[] returnBytes = new byte[str.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);
            return returnBytes;
        }
    }
}
