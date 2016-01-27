using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class Student
    {
        //public int ID { get; set; }
        //public string LastName { get; set; }
        //public string FirstMidName { get; set; }
        //public DateTime EnrollmentDate { get; set; }
        //public string City { get; set; }
        //public string Country { get; set; }
        //public virtual ICollection<Enrollment> Enrollments { get; set; }

        public Guid StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Degree { get; set; }
        public string University { get; set; }
        public string AboutMe { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public float CurrentBalance { get; set; }

    }
}