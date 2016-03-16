using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public class Tutor: User
    {
        public Tutor()
        {
            role = "Tutor";
        }
        private string AboutMe { get; set; }
        private string Experience { get; set; }
        private float CurrentEarning { get; set; }
        private Boolean IsCompletedProfile { get; set; }
        private float Rating { get; set; }
        private string paypalEmail { get; set; }
        private string moneyStatus { get; set; }

        bool login(string username, string password) {

            return false;

        }
        bool Register(User obj)
        {
            return false;
        }

        public virtual ICollection<Category> tutorExperties { get; set; }
        public virtual ICollection<Session> ActiveSessions { get; set; }
    }
}