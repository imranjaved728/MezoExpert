using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using WebApplication2.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using WebApplication2.DBEntities;

namespace SignalRChat
{
    public class Onlines : Hub
    {
        public void Send(string userId, string message)
        {
            Clients.All.send(message);
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;
            using (var db = new ApplicationDbContext())
            {
                var user = db.online                 
                    .SingleOrDefault(u => u.Username == name);

                if (user == null)
                {
                    user = new OnlineUsers
                    {
                        Username = name,
                        Status = true
                    };
                    db.online.Add(user);
                }
                              
                else
                {
                    user.Status = true;
                    db.Entry(user).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new ApplicationDbContext())
            {
                var name = Context.User.Identity.Name;
                var connection = db.online.Find(name);
                connection.Status = false;
                
                db.Entry(connection).State = EntityState.Modified;


                db.SaveChanges();
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}