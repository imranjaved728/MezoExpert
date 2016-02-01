using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication2.Models
{
    public class QuestionViewModel
    {
        public Guid QuestionID { get; set; }
        public Guid StudentID { get; set; }
        public Guid? TutorID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [AllowHtml]
        public string Details { get; set; }

        public string Status { get; set; }
        [Required]
        public float Amount { get; set; }

        public Guid? CategoryID { get; set; }

       
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }
        public DateTime PostedTime { get; set; }
    }
}