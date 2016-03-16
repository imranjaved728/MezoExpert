using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Admin:User
    {
        public Admin()
        {
            role = "Admin";
        }

        bool login(string username, string password)
        {

            return false;

        }
        bool Register(User obj)
        {
            return false;
        }

        public virtual ICollection<Category> tutorExperties { get; set; }
        public virtual ICollection<Session> ActiveSessions { get; set; }
        public virtual ICollection<Tutor> Tutors { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Transaction> Payments { get; set; }
        public virtual ICollection<Question> questions { get; set; }
    }
}