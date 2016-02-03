using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2.DBEntities;

namespace WebApplication2.Models
{
    public class Category
    {
        public Guid CategoryID { get; set; }
        public String CategoryName { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<TutorExperties> tutorExperties { get; set; }
    }
}