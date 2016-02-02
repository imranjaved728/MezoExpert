using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class Tutor
    {
        public Guid TutorID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Degree { get; set; }
        public string University { get; set; }
        public string AboutMe { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime DateCreated { get; set; }
        public float CurrentEarning { get; set; }
        public string ProfileImage { get; set; }


    }

   
    //public enum Grade
    //{
    //    A, B, C, D, F
    //}

    //public class Enrollment
    //{
    //    public int EnrollmentID { get; set; }
    //    public int CourseID { get; set; }
    //    public int StudentID { get; set; }
    //    public Grade? Grade { get; set; }

    //    public virtual Course Course { get; set; }
    //    public virtual Student Student { get; set; }
    //}

    //public class Course
    //{
    //    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    //    public int CourseID { get; set; }
    //    [Required]
    //    public string Title { get; set; }
    //    public int Credits { get; set; }

    //    public virtual ICollection<Enrollment> Enrollments { get; set; }
    //}
}