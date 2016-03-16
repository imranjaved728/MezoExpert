using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    //command pattern
    public class RecieveMessage : Command
    {
        private Reply obj;

        public RecieveMessage(Reply v)
        {
            obj = v;
        }

        public void Execute(string message)
        {
            obj.RecieveMessage(message);
        }
    }
}