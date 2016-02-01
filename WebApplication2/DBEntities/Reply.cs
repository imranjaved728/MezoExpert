using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.DBEntities
{
    public class Reply
    {
        public Guid ReplyID { get; set; }
        public Guid QuestionID { get; set; }
        public Guid ReplierID { get; set; }
        public string Details { get; set; }
        public DateTime PostedTime { get; set; }

        public virtual ICollection<Files> Files { get; set; }
    }
}