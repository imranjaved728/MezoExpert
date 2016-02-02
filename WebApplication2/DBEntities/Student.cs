using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class Student
    {
        public Guid StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Degree { get; set; }
        public string University { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime DateCreated { get; set; }
        public float CurrentBalance { get; set; }
        public string ProfileImage { get; set; }

    }
}