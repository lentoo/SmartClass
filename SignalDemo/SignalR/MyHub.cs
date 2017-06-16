using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalDemo.SignalR
{
    [HubName("MyHub")]
    public class MyHub : Hub
    {
        public void Send(string name,string message)
        {
            Clients.Others.Reciver(name,message);
        }
    }
}