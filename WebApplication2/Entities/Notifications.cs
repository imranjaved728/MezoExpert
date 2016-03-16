using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Notifications
    {
        public Guid ID { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public bool isRead { get; set; }

        public string sessionId { get; set; }

        public int counts { get; set; } //for future

        public string link { get; set; }

        public DateTime postedTime { get; set; }


        public Notifications()
        {
            ID = new Guid();
        }
    }
}