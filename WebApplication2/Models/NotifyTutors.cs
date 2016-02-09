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
    public class NotifyTutors : Hub
    {
        public void Send(string message)
        {
            // Call the broadcastMessage method to update tutors only.
            // Clients.All.addNewMessageToPage(name, message);  //send to all
            string roomName = "tutors";
            Clients.Group(roomName).addNewMessageToPage(message);
        }
       

    public override Task OnConnected()
    {
         //adding expert to group
        if(Context.User.IsInRole("Tutor"))
         { 
                string roomName = "tutors";
                Groups.Add(Context.ConnectionId, roomName);

         }
        
        return base.OnConnected();
    }

    //public override Task OnDisconnected(bool stopCalled)
    //{
    //    using (var db = new ApplicationDbContext())
    //    {
    //        var connection = db.Connections.Find(Context.ConnectionId);
    //        connection.Connected = false;
    //        db.SaveChanges();
    //    }
    //    return base.OnDisconnected(stopCalled);
    //}
}
}