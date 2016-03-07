using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication2.DBEntities;
using WebApplication2.Models;

namespace WebApplication2.Helpers
{
    public class notifications:Hub
    {
        public void Send(string userId, string message)
        {
            Clients.All.send(message);
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;
            if (name != "")
            {
                using (var db = new ApplicationDbContext())
                {
                    var user = db.notifyConnections
                        .SingleOrDefault(u => u.userName == name);

                    if (user == null)
                    {
                        user = new NotificationConnections
                        {
                            ID = Guid.NewGuid(),
                            userName = name,
                            connectionId = Context.ConnectionId
                        };
                        db.notifyConnections.Add(user);
                    }

                    else
                    {
                        user.connectionId = Context.ConnectionId;
                        db.Entry(user).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            /*using (var db = new ApplicationDbContext())
            {
                var name = Context.User.Identity.Name;
                if (name != "")
                {

                    var connection = db.online.Find(name);
                    connection.Status = false;

                    db.Entry(connection).State = EntityState.Modified;


                    db.SaveChanges();
                }
            }*/
            return base.OnDisconnected(stopCalled);
        }
    }
}