using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SmartClass.Infrastructure.Extended;
namespace SerialPortServer
{
    public class SocketServer
    {
        private AppServer appServer;
        public void Start()
        {
            if (appServer != null)
            {
                if (appServer.Start())
                {                    
                    Console.WriteLine("开始监听");
                }
            }
        }
        Socket socketServer;
        byte[] datas = new byte[1024];
        public SocketServer(int listenPort, int maxConnect, string textEncoding = "UTF-8")
        {
            appServer = new AppServer();
            ServerConfig config = new ServerConfig();
            config.Port = listenPort;
            config.TextEncoding = textEncoding;
            config.MaxConnectionNumber = maxConnect;
            if (!appServer.Setup(config))
            {
                Console.WriteLine($"{listenPort} :端口占用");
            }
            appServer.NewSessionConnected += AppServer_NewSessionConnected;
            appServer.NewRequestReceived += AppServer_NewRequestReceived;
            appServer.SessionClosed += AppServer_SessionClosed;
            //socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint point = new IPEndPoint(IPAddress.Any, listenPort);
            //socketServer.Bind(point);
            //socketServer.Listen(maxConnect);

        }
        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        private void AppServer_SessionClosed(AppSession session, CloseReason value)
        {
            Logger($"{session.RemoteEndPoint} :已断开连接");
        }
        /// <summary>
        /// 新的会话连接
        /// </summary>
        /// <param name="session"></param>
        private void AppServer_NewSessionConnected(AppSession session)
        {
            Logger($"{session.RemoteEndPoint} :已连接");

        }
        /// <summary>
        /// 接收到新数据
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        private void AppServer_NewRequestReceived(AppSession session, SuperSocket.SocketBase.Protocol.StringRequestInfo requestInfo)
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(requestInfo.Key);
            foreach (var item in requestInfo.Parameters)
            {
                sb.Append(" " + item);
            }
           
            string data = sb.ToString();

            ProcessReceivedData(session,data);
        }


        private void Logger(string log)
        {
            Console.WriteLine(log);
        }
        public async Task BeginListen()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("开始等待客户端连接");

                    Socket socket = socketServer.Accept();
                    Console.WriteLine(socket.RemoteEndPoint + "：已连接");
                    await Task.Run(() =>
                     {
                         while (true)
                         {
                             try
                             {
                                 Console.WriteLine("准备接收数据");
                           //接收客户端发来的数据
                           socket.ReceiveTimeout = 3000;
                                 int len = socket.Receive(datas);
                                 if (len == 0)
                                 {
                                     Console.WriteLine($"客户端：{socket.RemoteEndPoint}断开连接");
                                     socket.Close();
                                     socket.Dispose();
                                     break;
                                 }
                                 Console.WriteLine("接收到数据，长度为" + len);
                           //教室地址
                           string classroom = Convert.ToString(datas[2], 16) + Convert.ToString(datas[3], 16);
                                 byte[] data = new byte[len];
                                 Array.Copy(datas, 0, data, 0, len);

                           //处理接收到的数据
                           ProcessReceivedData(socket, classroom, data);
                             }
                             catch (Exception ex)
                             {
                                 Console.WriteLine("通信出现异常：" + ex.Message);
                                 Console.WriteLine($"{socket.RemoteEndPoint}断开连接");
                                 socket.Close();
                                 socket.Dispose();
                                 break;
                             }
                         }
                     });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("出现异常:" + ex.Message);
                }
                finally
                {
                    ;
                }
            }
        }
        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        public void ProcessReceivedData(AppSession session,string data)        {
            
            string str = data;
            switch (str)
            {
                case "报警数据":
                    byte[] bs = null;
                    Logger("查询报警数据");
                    if (SerialPortUtils.AlarmData.Count > 0) //有报警数据
                    {
                        bs = SerialPortUtils.AlarmData.Dequeue();
                    }
                    else
                    {
                        bs = Encoding.UTF8.GetBytes("0\r\n");
                    }
                    session.Send(bs,0,bs.Length);
                    break;
                default:  //默认是查询向串口查询教室设备数据
                          //发送数据
                    var dataBuff = data.StrToHexByte();
                    string classroom = Convert.ToString(dataBuff[2], 16) + Convert.ToString(dataBuff[3], 16);
                    if (dataBuff.Length > 4)
                    {
                        if (dataBuff[4] == 0x1f)  //接收到的是查询命令
                        {
                            SerialPortUtils.SendSearchCmd(dataBuff);
                            //获取串口返回的数据
                            byte[] returnData = GetReturnData(classroom);
                            //将获取到的数据发送回去
                            session.Send(returnData.HexToStr() + "\r\n");
                            Console.WriteLine("服务端给客户端发送完成数据：" + session.RemoteEndPoint);
                        }
                        else       //接收到的是执行命令
                        {
                            Console.WriteLine("接收到执行命令，发送完成");
                            SerialPortUtils.SendCmd(dataBuff);
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        public void ProcessReceivedData(Socket socket, string classroom, byte[] data)
        {
            string str = Encoding.UTF8.GetString(data, 0, data.Length);
            switch (str)
            {
                case "报警数据":
                    byte[] bs = null;
                    if (SerialPortUtils.AlarmData.Count > 0) //有报警数据
                    {
                        bs = SerialPortUtils.AlarmData.Dequeue();
                    }
                    else
                    {
                        bs = new byte[] { 0 };
                    }
                    socket.Send(bs);
                    break;
                default:  //默认是查询向串口查询教室设备数据
                          //发送数据
                    if (data.Length > 4)
                    {
                        if (data[4] == 0x1f)  //接收到的是查询命令
                        {
                            SerialPortUtils.SendSearchCmd(data);
                            //获取串口返回的数据
                            byte[] returnData = GetReturnData(classroom);
                            //将获取到的数据发送回去
                            socket.Send(returnData);

                            Console.WriteLine("服务端给客户端发送完成数据：" + socket.RemoteEndPoint);
                        }
                        else       //接收到的是执行命令
                        {
                            Console.WriteLine("接收到执行命令，发送完成");
                            SerialPortUtils.SendCmd(data);
                        }
                    }
                    break;
            }
        }
        public byte[] GetReturnData(string classroom)
        {

            //等待数据初始化

            SerialPortUtils.countdown.Wait(TimeSpan.FromSeconds(5));
            SerialPortUtils.countdown.Reset();
            if (!SerialPortUtils.DataDictionary.ContainsKey(classroom))
            {
                Console.WriteLine("-------获取串口数据失败，返回1个长度数据包-----");
                return new byte[] { 0 };
            }
            Console.WriteLine("=========获取到串口完整数据=======");
            Console.WriteLine($"字典key为：{classroom} 的数据被消费");
            byte[] data = SerialPortUtils.DataDictionary[classroom];
            SerialPortUtils.DataDictionary.Remove(classroom);
            //TODO 用处：测试BUG，
            SerialPortUtils.SendCmd(data);
            return data;
        }

    }
}
