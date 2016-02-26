using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class StudentInbox
    {
        public Guid SessionId { get; set; }
        public string SenderName { get; set; }
        public string LastMessage { get; set; }
        public string Status { get; set; }
        public string SenderProfile { get; set; }
        public bool newMessage { get; set; }
        public DateTime PostedTime { get; set; }
    }

    public class StudentHomeModel
    {
        public StudentHomeModel()
        {
            obj = new List<StudentInbox>();
        }

        public List<StudentInbox> obj;
        public List<Question> questions { get; set; }
    }
}