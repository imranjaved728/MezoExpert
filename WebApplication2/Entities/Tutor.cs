using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2;
using WebApplication2.Models;

namespace WebApplication3.Entities
{
    public class Tutor: User
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;

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

            var user = _dbContext.Tutors.Where(c => c.Username == username && password == password);

            if (user == null)
                return false;
            else
                return true;
        }

        bool Register(User obj)
        {
            Tutor t = (Tutor)obj;

            Tutor stu = new Tutor();
            stu.ID = Guid.NewGuid();
            stu.DateCreated = DateTime.Today;
            Random rnd = new Random();
            int filename = rnd.Next(1, 4);
            stu.ProfileImage = "/Profiles/default/" + filename + ".png";
            stu.Username = t.Username;


            Notifications notify = new Notifications();
            notify.ID = Guid.NewGuid();
            notify.isRead = false;
            notify.Message = "/Profiles/default/admin.png^Admin^You have successfully created your account. ";
            notify.UserName = stu.Username;
            notify.postedTime = DateTime.Now;


            Notifications notify2 = new Notifications();
            notify2.ID = Guid.NewGuid();
            notify2.isRead = false;
            notify2.Message = "/Profiles/default/admin.png^Admin^We now have Arabic Language support as well.";
            notify2.UserName = stu.Username;
            notify2.postedTime = DateTime.Now;


            _dbContext.SaveChanges();
            return true;
        }

        public virtual ICollection<Category> tutorExperties { get; set; }
        public virtual ICollection<Session> ActiveSessions { get; set; }
    }
}