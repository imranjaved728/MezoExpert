using ClassLibrary1;
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
        public string StudentName { get; set; }
        public string StudentProfile { get; set; }

        public Guid? TutorID { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources),
              ErrorMessageResourceName = "titleRequired")]
   
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long and maximum 256 characters long.", MinimumLength = 10)]
        //[Display(Name = "Title", ResourceType = typeof(Resource))]
        public string Title { get; set; }

       
        [AllowHtml]
        [Required(ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "detailRequired")]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} characters long and maximum 1000 characters long.", MinimumLength = 10)]
        public string Details { get; set; }

        public string Status { get; set; }
     

        [Required(ErrorMessageResourceType = typeof(Resources),
            ErrorMessageResourceName = "amountRequired")]
        public float Amount { get; set; }

        public Guid? CategoryID { get; set; }
        public string CategoryName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }
        public DateTime PostedTime { get; set; }
       
    }
}