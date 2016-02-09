using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class TutorQuestionDetails
    {
        public Guid QuestionID { get; set; }

        [AllowHtml]
        public string replyDetails { get; set; }
         
        public Question question { get; set; }
        public Session session { get; set; }
        public Student student { get; set; }
        public Tutor tutor { get; set; }
    }
}