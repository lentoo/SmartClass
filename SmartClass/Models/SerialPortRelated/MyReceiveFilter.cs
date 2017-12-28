using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SmartClass.Models.SerialPortRelated
{
    public class MyReceiveFilter : TerminatorReceiveFilter<StringPackageInfo>
    {
        public MyReceiveFilter() : base(Encoding.ASCII.GetBytes("\r\n"))
        {
        }

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            string str = bufferStream.ReadString((int)bufferStream.Length, Encoding.UTF8);
            return new StringPackageInfo(str, str, str.Split(' '));
        }
    }
}