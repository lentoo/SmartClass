using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using SmartClass.Infrastructure.Cache;
using SmartClass.Infrastructure.Mac;
using SmartClass.Models.AutofacConfig;
using System.Diagnostics;

namespace SmartClass.Models.SignalR
{
  [HubName("QRCodeHub")]
  public class QRCodeHub : Hub
  {
    public QRCodeHub(ICacheHelper Cache)
    {
      this.Cache = Cache;
    }
    private ICacheHelper Cache;
    /// <summary>
    /// 客户端连接
    /// </summary>
    public void Connection()
    {
      string mac = MacUtils.GetClientMAC(HttpContext.Current.Request);
      Cache.DeleteCache(mac);
      string id = Context.ConnectionId;
      Debug.WriteLine($" Connected : mac = {mac} ; id = {id}");
      //将客户端websocket Id存起来
      Cache.SetCache(mac, id);
      Clients.Client(id).Reciver(mac, id);
    }
    /// <summary>
    ///  客户端断开连接
    /// </summary>
    /// <param name="stopCalled"></param>
    /// <returns></returns>
    public override Task OnDisconnected(bool stopCalled)
    {
      string id = Context.ConnectionId;
      Debug.WriteLine($" Disconnected : {id}");
      //删除缓存数据
      Cache.DeleteCache<string>(Context.ConnectionId);
      return base.OnDisconnected(stopCalled);
    }

    /// <summary>
    ///  客户端重新连接
    /// </summary>
    /// <returns></returns>
    public override Task OnReconnected()
    {
      string mac = MacUtils.GetClientMAC(HttpContext.Current.Request);
      string id = Context.ConnectionId;
      Debug.WriteLine($" Reconnected : mac = {mac} ; id = {id}");
      return base.OnReconnected();
    }
  }
}