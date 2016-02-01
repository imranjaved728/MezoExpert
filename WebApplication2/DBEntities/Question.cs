using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebApplication2.Models;

namespace WebApplication2.DBEntities
{
    public class Question
    {
        public Guid QuestionID { get; set; }

    
        public Guid StudentID { get; set; }
       
        public Guid? TutorID { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
        public float Amount { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DueDate { get; set; }
        public DateTime PostedTime { get; set; }

        public virtual Student student { get; set; }
        public virtual ICollection<Reply> Replies { get; set; }
        public virtual ICollection<Files> Files { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}