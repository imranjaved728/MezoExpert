using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Reply
    {
        public Reply()
        {
            ReplyID = new Guid();
        }

        public Guid ReplyID { get; set; }
        public Guid ReplierID { get; set; }
        public string Details { get; set; }
        public DateTime PostedTime { get; set; }

        public string SendMessage()
        {
            return Details;
        }

        public void RecieveMessage(string message)
        {
            Details = message;
        }


        public virtual ICollection<Files> Files { get; set; }
    }
}