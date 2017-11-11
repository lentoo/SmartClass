using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SerialPortServer
{
  public class ConsoleWin32Helper
  {
    public static bool _IsExit = false;
    static ConsoleWin32Helper()
    {
      _NotifyIcon.Icon = new Icon(@"..\..\icon.ico");
      _NotifyIcon.Visible = false;
      _NotifyIcon.Text = "tray";

      ContextMenu menu = new ContextMenu();
      MenuItem item = new MenuItem();
      item.Text = "退出程序";
      item.Index = 0;
      item.Click += Item_Click;


      menu.MenuItems.Add(item);
      _NotifyIcon.ContextMenu = menu;

      _NotifyIcon.MouseClick += new MouseEventHandler(_NotifyIcon_MouseClick);

    }

    private static void Item_Click(object sender, EventArgs e)
    {
      _IsExit = true;
    }

    static void _NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
      Console.WriteLine("托盘被双击.");
    }

    #region 禁用关闭按钮
    [DllImport("User32.dll", EntryPoint = "FindWindow")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
    static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

    [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
    static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    ///<summary>
    /// 禁用关闭按钮
    ///</summary>
    ///<param name="consoleName">控制台名字</param>
    public static void DisableCloseButton(string title)
    {
      //线程睡眠，确保closebtn中能够正常FindWindow，否则有时会Find失败。。
      Thread.Sleep(100);

      IntPtr windowHandle = FindWindow(null, title);
      IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
      uint SC_CLOSE = 0xF060;
      RemoveMenu(closeMenu, SC_CLOSE, 0x0);
    }
    public static bool IsExistsConsole(string title)
    {
      IntPtr windowHandle = FindWindow(null, title);
      if (windowHandle.Equals(IntPtr.Zero)) return false;

      return true;
    }
    #endregion

    #region 托盘图标
    static NotifyIcon _NotifyIcon = new NotifyIcon();
    public static void ShowNotifyIcon()
    {
      _NotifyIcon.Visible = true;
      _NotifyIcon.ShowBalloonTip(3000, "", "我是串口服务程序，请保持常开，右键可关闭。", ToolTipIcon.Info);      
    }
    public static void CloseNotifyIcon()
    {
      _NotifyIcon.Visible = false ;
    }
    public static void HideNotifyIcon()
    {
      _NotifyIcon.Visible = false;
    }

    #endregion
  }
}
