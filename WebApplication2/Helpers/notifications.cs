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
        private ApplicationDbContext db = new ApplicationDbContext();

        public void Send(string userId, string message)
        {
            Clients.All.send(message);
        }

        /*    public void senderTest(string sessionId,string replyDetail)
            {
                var user = Context.User;

                if (user.Identity.IsAuthenticated)
                {
                    string username = user.Identity.Name;
                    var id = db.Students.Where(c => c.Username == username).FirstOrDefault().StudentID;

                    string messagefinal = replyDetail;
                    Reply obj = new Reply();
                    obj.ReplyID = Guid.NewGuid();
                    obj.ReplierID = id;
                    obj.SessionID = new Guid(sessionId);
                    obj.PostedTime = DateTime.Now;
                    messagefinal = messagefinal.Replace(Environment.NewLine, "<br/>");
                    obj.Details = messagefinal;

                    db.Replies.Add(obj);

                    var session = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault();
                    session.NewMessageTutor = true;
                    db.Entry(session).State = EntityState.Modified;

                     db.SaveChangesAsync();


                    var imgsrc = db.Students.Where(c => c.Username == username).FirstOrDefault().ProfileImage;

                    //part moved up
                    var tutor = db.sessions.Where(c => c.SessionID == obj.SessionID).FirstOrDefault().tutor;
                    var username2 = tutor.Username;
                    var status = db.online.Where(c => c.Username == username2).FirstOrDefault().Status;

                    string message = generateMessage(username, obj.Details, imgsrc, obj.PostedTime.ToString(), obj.ReplyID.ToString(), status);
                    SendChatMessageStudentReciever(obj.SessionID.ToString(), username, message); //send message to urself 

                    SendChatMessageTutorReciever(obj.SessionID.ToString(), username2, message); //send message to other person 

                }
            }

            public string generateMessage(string username, string detail, string imgsrc, string postedTime, string replyID, bool online)
            {
                string filestring = "<div id=\"" + replyID + "\"></div>";

                string message = "";
                message = message + "<li class=\"" + online + "\"></li><li class=\"media\">" +
                                "<div class=\"comment\"> " +
                                        "<a href=\"#\" class=\"pull-left\"><img src=\"" + imgsrc + "\" alt=\"\" class=\"img-circle imgSize\"> </a>" +
                                         " <div class=\"media-body\">" +
                                         " <a style=\"text-decoration:none\" href=\"@Url.Action(\"Profile\", \"Home\", new { username = tutor.Username })\"> <strong class=\"text-success userText username\">" + username + "</strong><br /><br /></a>" +
                                           detail +
                                          filestring +
                                         "<div class=\"clearfix\"></div>" +
                                         " </div>" +
                                         "<div style=\"margin-bottom:20px\">" +
                                                  "<small class=\"text-muted pull-right\">" + postedTime + "</small>" +
                                           " </div>" +
                                         "<hr>" +
                                   "</div>" +
                                   "</li>";
                return message;
            }

            public void SendChatMessageStudentReciever(string sessionId, string sendTo, string message)
            {
                //var name = Context.User.Identity.Name;
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                    if (user == null)
                    {
                        // context.Clients.Caller.showErrorMessage("Could not find that user.");
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
                            //  Clients.Caller.showErrorMessage("The user is no longer connected.");
                        }
                        else
                        {
                            foreach (var connection in user.Connections)
                            {
                                Clients.Client(connection.ConnectionID)
                                     .recieverStudent(message);
                            }
                        }
                    }
                }
            }

            public void SendChatMessageTutorReciever(string sessionId, string sendTo, string message)
            {
                //var name = Context.User.Identity.Name;
                using (var db = new ApplicationDbContext())
                {
                    var user = db.Useras.Where(c => c.UserName == sendTo && c.SessionId == sessionId).FirstOrDefault();
                    if (user == null)
                    {
                        // context.Clients.Caller.showErrorMessage("Could not find that user.");
                        var session = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault();
                        var Username = session.question.student.Username;
                        var img = session.question.student.ProfileImage;

                        var notiAlready = db.notifications.Where(c => c.sessionId == sessionId && c.UserName == sendTo).FirstOrDefault();
                        if (notiAlready == null)
                        {
                            Notifications notify = new Notifications();
                            notify.ID = Guid.NewGuid();
                            notify.isRead = false;
                            notify.Message = session.question.student.ProfileImage + "^" + Username + "^" + "has sent you a message.";
                            notify.UserName = sendTo;
                            notify.sessionId = sessionId;
                            notify.postedTime = DateTime.Now;
                            db.notifications.Add(notify);

                        }
                        else
                        {
                            notiAlready.counts = notiAlready.counts + 1;
                            notiAlready.isRead = false;
                            db.Entry(notiAlready).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                        SendNotification(sendTo, Username, session.question.student.ProfileImage, "has sent you a message.", false);
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
                            var session = db.sessions.Where(c => c.SessionID == new Guid(sessionId)).FirstOrDefault();
                            var Username = session.question.student.Username;
                            var img = session.question.student.ProfileImage;

                            var notiAlready = db.notifications.Where(c => c.sessionId == sessionId && c.UserName == sendTo).FirstOrDefault();
                            if (notiAlready == null)
                            {
                                Notifications notify = new Notifications();
                                notify.ID = Guid.NewGuid();
                                notify.isRead = false;
                                notify.Message = session.question.student.ProfileImage + "^" + Username + "^" + "has sent you a message.";
                                notify.UserName = sendTo;
                                notify.sessionId = sessionId;
                                notify.postedTime = DateTime.Now;
                                db.notifications.Add(notify);

                            }
                            else
                            {
                                notiAlready.counts = notiAlready.counts + 1;
                                notiAlready.isRead = false;
                                db.Entry(notiAlready).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            SendNotification(sendTo, Username, session.question.student.ProfileImage, "has sent you a message.", false);
                        }

                        else
                        {
                            foreach (var connection in user.Connections)
                            {
                                Clients.Client(connection.ConnectionID)
                                     .recieverStudent2(message);
                            }


                        }
                    }
                }
            }

            private void SendNotification(string sendTo, string username, string image, string message, bool addDb)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<notifications>();
                //var name = Context.User.Identity.Name;
                using (var db = new ApplicationDbContext())
                {
                    var user = db.notifyConnections.Where(c => c.userName == sendTo).FirstOrDefault();
                    if (user != null)
                    {

                        string finalmessage = image + "^" + username + "^" + message;

                        if (addDb == true)
                        {
                            Notifications notify = new Notifications();
                            notify.ID = Guid.NewGuid();
                            notify.isRead = false;
                            notify.Message = finalmessage;
                            notify.UserName = sendTo;
                            notify.postedTime = DateTime.Now;
                            db.notifications.Add(notify);
                            db.SaveChanges();
                        }
                        var counter = db.notifications.Where(c => c.UserName == sendTo && c.isRead == false).Count();

                        finalmessage = finalmessage + "^" + counter;

                        context.Clients.Client(user.connectionId)
                                     .recieverNotifier(finalmessage);


                    }
                }
            }*/

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