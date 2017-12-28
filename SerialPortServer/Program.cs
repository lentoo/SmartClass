using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortServer
{
    class Program
    {
        static void Main(string[] args) => MainResult();

        static void MainResult()
        {
            //ConsoleWin32Helper.ShowNotifyIcon();
            //ConsoleWin32Helper.DisableCloseButton(Console.Title);
            SerialPortUtils.InitialSerialPort();
            Console.WriteLine("程序启动");
            SocketServer socketServer = new SocketServer(8081, 1024);

            socketServer.Start();
            //var task = Task.Run(async () =>
            //{
            //    Console.WriteLine($" async ThreadId {Thread.CurrentThread.ManagedThreadId}");
            //    await socketServer.BeginListen();
            //});
            //Console.WriteLine($" sync ThreadId {Thread.CurrentThread.ManagedThreadId}");
            Console.ReadKey();
            //while (true)
            //{
            //  Application.DoEvents();
            //  if (ConsoleWin32Helper._IsExit)
            //  {
            //    MonitorInput();
            //    break;
            //  }
            //}
        }
        static void MonitorInput()
        {
            ConsoleWin32Helper._IsExit = true;
            ConsoleWin32Helper.CloseNotifyIcon();
            Application.ExitThread();
        }
    }
}
