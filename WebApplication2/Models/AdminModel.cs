using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class AdminModel
    {
        public int TutorCount { get; set; }
        public int StudentCount { get; set; }
        public int QuestionCount { get; set; }
        public int SessionCount { set; get; }
        public int CategoriesCount { set; get; }
        public int complaintsCount { set; get; }
    }
}