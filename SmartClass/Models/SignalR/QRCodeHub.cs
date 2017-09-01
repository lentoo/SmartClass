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
    //客户端连接
    public void Connection()
    {
      string mac = MacUtils.GetClientMAC(HttpContext.Current.Request);
      Cache.DeleteCache(mac);
      string id = Context.ConnectionId;
      //将客户端websocket Id存起来
      Cache.SetCache(mac, id);
      Clients.Client(id).Reciver(mac, id);
    }

    public override Task OnDisconnected(bool stopCalled)
    {
      //删除缓存数据
      Cache.DeleteCache<string>(Context.ConnectionId);
      return base.OnDisconnected(stopCalled);
    }
  }
}