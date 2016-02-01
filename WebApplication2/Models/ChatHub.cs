using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using WebApplication2.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
    //    public void Send(string name, string message)
    //    {
    //        // Call the broadcastMessage method to update clients.
    //        Clients.All.addNewMessageToPage(name, message);
    //    }
    //    public void SendChatMessage(string who, string message)
    //    {
    //        string name = Context.User.Identity.Name;

    //        Clients.Group(who).addChatMessage(name + ": " + message);
    //      //  Clients.All.addChatMessage(name, message);
    //    }

    //    public override Task OnConnected()
    //    {
    //        string name = Context.User.Identity.Name;

    //        Groups.Add(Context.ConnectionId, name);

    //        return base.OnConnected();
    //    }
    //}

    public void SendChatMessage(string who, string message)
    {
        var name = Context.User.Identity.Name;
        using (var db = new ApplicationDbContext())
        {
            var user = db.Useras.Find(who);
            if (user == null)
            {
                Clients.Caller.showErrorMessage("Could not find that user.");
            }
            else
            {
                db.Entry(user)
                    .Collection(u => u.Connections)
                    .Query()
                    .Where(c => c.Connected == true)
                    .Load();

                if (user.Connections == null)
                {
                    Clients.Caller.showErrorMessage("The user is no longer connected.");
                }
                else
                {
                    foreach (var connection in user.Connections)
                    {
                        Clients.Client(connection.ConnectionID)
                            .addChatMessage(name + ": " + message);
                    }
                }
            }
        }
    }

    public override Task OnConnected()
    {
        var name = Context.User.Identity.Name;
        using (var db = new ApplicationDbContext())
        {
            var user = db.Useras
                .Include(u => u.Connections)
                .SingleOrDefault(u => u.UserName == name);

            if (user == null)
            {
                user = new ApplicationDbContext.Usera
                {
                    UserName = name,
                    Connections = new List<ApplicationDbContext.Connection>()
                };
                db.Useras.Add(user);
            }

            user.Connections.Add(new ApplicationDbContext.Connection
            {
                ConnectionID = Context.ConnectionId,
                UserAgent = Context.Request.Headers["User-Agent"],
                Connected = true
            });
            db.SaveChanges();
        }
        return base.OnConnected();
    }

    public override Task OnDisconnected(bool stopCalled)
    {
        using (var db = new ApplicationDbContext())
        {
            var connection = db.Connections.Find(Context.ConnectionId);
            connection.Connected = false;
            db.SaveChanges();
        }
        return base.OnDisconnected(stopCalled);
    }
}
}