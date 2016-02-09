using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class ChatModel
    {
        public Session session { get; set; }
        [AllowHtml]
        public String  replyDetail { get; set; }
        public Guid sessionID { get; set; }
    }
}