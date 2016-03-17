using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2;
using WebApplication2.Models;

namespace WebApplication3.Entities
{
    public class SendMessage : Command
    {
        private Reply obj;

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;

        public SendMessage(Reply v)
        {
            obj = v;

        }

        public void Execute(string message)
        {
            var session = obj.ReplyID;
            _dbContext.sessions.Where(c => c.SessionID == session);

            _dbContext.SaveChanges();
            
            obj.SendMessage();
        }
    }
}