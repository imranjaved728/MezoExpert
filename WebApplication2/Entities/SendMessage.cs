using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class SendMessage : Command
    {
        private Reply obj;

        public SendMessage(Reply v)
        {
            obj = v;

        }

        public void Execute(string message)
        {
            obj.SendMessage();
        }
    }
}