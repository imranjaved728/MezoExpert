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
    public class TutorStudentChat : Hub
    {
        public void Send(string message)
        {
            // Call the broadcastMessage method to update tutors only.
            // Clients.All.addNewMessageToPage(name, message);  //send to all
            string roomName = "tutors";
            Clients.Group(roomName).addNewMessageToPage(message);
        }


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
                                .reciever(name + ": " + message);
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
                string SessionId = Context.QueryString["SessionId"];
               
                //all messages read
                var session = db.sessions.Where(c => c.SessionID == new Guid(SessionId)).FirstOrDefault();
                if(name==session.tutor.Username)
                    session.NewMessageTutor = false;
                else
                     session.NewMessageStudent = false;
                db.Entry(session).State = EntityState.Modified;

                var user = db.Useras
                    .Include(u => u.Connections)
                    .SingleOrDefault(u => u.UserName == name && u.SessionId == SessionId);

                if (user == null)
                {
                    user = new ApplicationDbContext.Usera
                    {
                        UserName = name,
                        SessionId= SessionId,
                        Connections = new List<ApplicationDbContext.Connection>()
                    };
                    db.Useras.Add(user);
                }

                if(user.Connections.Count==0)
                { 
                        user.Connections.Add(new ApplicationDbContext.Connection
                        {
                            ConnectionID = Context.ConnectionId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                            Connected = true
                        });
                }
                else
                {
                    var connection = user.Connections.FirstOrDefault();
                    db.Connections.Remove(connection);
                    user.Connections.Add(new ApplicationDbContext.Connection
                    {
                        ConnectionID = Context.ConnectionId,
                        UserAgent = Context.Request.Headers["User-Agent"],
                        Connected = true
                    });
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
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.Connected = false;

                string SessionId = Context.QueryString["SessionId"];
                var session = db.sessions.Where(c => c.SessionID == new Guid(SessionId)).FirstOrDefault();
                if (name == session.tutor.Username)
                    session.NewMessageTutor = false;
                else
                    session.NewMessageStudent = false;
                db.Entry(session).State = EntityState.Modified;


                db.SaveChanges();
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}