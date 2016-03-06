using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.Models;

namespace WebApplication2.DBEntities
{
    public class Session
    {
        public Guid SessionID { get; set; }
        //public Guid? StudentID { get; set; }
        public Guid? TutorID { get; set; }
        public Guid? QuestionID { get; set; }
        public DateTime PostedTime { get; set; }
        public Double OfferedFees { get; set; }
        public String Status { get; set; }

        public bool NewMessageTutor { get; set; }
        public bool NewMessageStudent { get; set; }
        public bool isClosed { get; set; }
        public bool isTutorDeleted { get; set; }
        public bool isStudentDelete { get; set; }
        public double? ratings { get; set; }

        public virtual Question question { get; set; }
        //public virtual Student student { get; set; }
        public virtual Tutor tutor { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
    }

   
}