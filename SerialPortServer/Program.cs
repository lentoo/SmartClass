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
    static void Main(string[] args) => MainResult().GetAwaiter().GetResult();


    static async Task MainResult()
    {

      //Console.Title = "TestConsoleLikeWin32";
      ConsoleWin32Helper.ShowNotifyIcon();
      ConsoleWin32Helper.DisableCloseButton(Console.Title);

      SerialPortUtils.InitialSerialPort();
      Console.WriteLine("程序启动");
      SocketServer socketServer = new SocketServer(8081, 1024);

      Task.Run(async () => await socketServer.BeginListen());


      while (true)
      {
        Application.DoEvents();
        if (ConsoleWin32Helper._IsExit)
        {
          MonitorInput();
          break;
        }
      }
      //Console.ReadKey();
    }
    static void MonitorInput()
    {
      ConsoleWin32Helper._IsExit = true;
      ConsoleWin32Helper.CloseNotifyIcon();
      Application.ExitThread();
    }
  }
}
