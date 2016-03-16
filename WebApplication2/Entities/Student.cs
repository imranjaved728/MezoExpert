using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Student:User
    {
        private float CurrentBalance { get; set; }
        public Student()
        {
            role = "Student";
        }
  
        bool login(string username, string password)
        {

            return false;

        }
        bool Register(User obj)
        {
            return false;
        }

        public virtual ICollection<Session> ActiveSessions { get; set; }
        public virtual ICollection<Question> QuestionsAsked { get; set; }
    }

}