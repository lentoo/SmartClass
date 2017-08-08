using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec;

namespace Common.Images
{
    /// <summary>
    /// 二维码帮助类
    /// </summary>
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>byte[]</returns>
        public static byte[] GetQRCode(string data)
        {
            QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE; //编码方式 (注意：BYTE能支持中文，ALPHA_NUMERIC扫描出来的都是数字)
            encoder.QRCodeScale = 20; //大小(值越大生成的二维码图片像素越高)
            encoder.QRCodeVersion = 0; //版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M; //错误效验、错误更正(有4个等级)
            //二维码数据
            string qrdata = data;
            System.Drawing.Bitmap bp = encoder.Encode(qrdata, Encoding.UTF8);
            MemoryStream ms = new MemoryStream();
            bp.Save(ms, ImageFormat.Png);
            return ms.GetBuffer();
        }
    }
}
