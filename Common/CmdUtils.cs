using System;

namespace Common
{
    public class CmdUtils
    {
        /// <summary>
        /// 通过命令获取CRC16转换后的执行器命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static byte[] ActuatorCommand(byte[] cmd)
        {
            //获取CRC校验码
            byte[] crcCode = CRC16.Crc(cmd);
            byte[] data = new byte[cmd.Length + 3];
            cmd.CopyTo(data, 0);
            data[data.Length - 1] = 0xbb;
            data[data.Length - 3] = crcCode[0];
            data[data.Length - 2] = crcCode[1];
            return data;
        }
        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>16进制字节数组</returns>
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
