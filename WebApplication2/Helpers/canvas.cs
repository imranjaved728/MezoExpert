using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication2.Helpers
{
    [Authorize]
    public class canvas : Hub
    {
       
            public void SendChatMessage(string who, string message)
            {
                Clients.Group(who).addChatMessage( message);
            }

            public override Task OnConnected()
            {
                string name = Context.User.Identity.Name;

                Groups.Add(Context.ConnectionId, name);

                return base.OnConnected();
            }
        }
    
}