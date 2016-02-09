using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class StudentInbox
    {
        public Guid SessionId { get; set; }
        public string SenderName { get; set; }
        public string LastMessage { get; set; }
        public string Status { get; set; }
        public DateTime PostedTime { get; set; }
    }
}