using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class TutorInbox
    {
        public IList<Session>sessions { get; set; }
        public IList<bool> online { get; set; }
        public TutorInbox()
        {
            sessions = new List<Session>();
            online= new List<bool>();
        }
    }
}