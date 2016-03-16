using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Question
    {
        public Question()
        {
            QuestionID = new Guid();
        }

        public Guid QuestionID { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
        public float Amount { get; set; }
       
        public DateTime? DueDate { get; set; }
        public DateTime PostedTime { get; set; }

        public virtual Student student { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
        //public virtual ICollection<Reply> Replies { get; set; }
        public virtual ICollection<Files> Files { get; set; }
        public virtual Category Category { get; set; }
    }
}