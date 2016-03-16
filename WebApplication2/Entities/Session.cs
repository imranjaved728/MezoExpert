using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Session
    {
        public Session()
        {
            SessionID = new Guid();
        }

        private Guid SessionID { get; set; }
        private DateTime PostedTime { get; set; }
        private Double OfferedFees { get; set;}
        private String Status { get; set; }
        
        //live action related to chat
        private bool isClosed { get; set; }
        private bool isTutorDeleted { get; set; }
        private bool isStudentDelete { get; set; }
        private double? ratings { get; set; }

        //lazy loading
        public List<Command> Commands { get; set; }
        public virtual Question question { get; set; }
        public virtual Transaction transaction { get; set; }
        public  virtual Student student { get; set; }
        public virtual Tutor tutor { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
    }
}