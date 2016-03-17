using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2;
using WebApplication2.Models;

namespace WebApplication3.Entities
{
    public class Admin:User
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;

        public Admin()
        {
            role = "Admin";
        }

        bool login(string username, string password)
        {
            var user = _dbContext.Users.Where(c => c.UserName == username && password == password);

            if (user == null)
                return false;
            else
                return true;

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