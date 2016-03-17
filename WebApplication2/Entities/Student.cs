using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication2;
using WebApplication2.Models;

namespace WebApplication3.Entities
{
    public class Student:User
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _dbContext;

        private float CurrentBalance { get; set; }
        public Student()
        {
            role = "Student";
        }
  
        bool login(string username, string password)
        {

            var user = _dbContext.Students.Where(c => c.Username == username && password == password);

            if (user == null)
                return false;
            else
                return true;
            

        }
        bool Register(User obj)
        {
            var roleresult = Microsoft.AspNet.Identity.UserManager.AddToRole(username, Status.Student);
            Student stu = new Student();
            stu.ID = Guid.NewGuid();
            stu.DateCreated = DateTime.Today;
            Random rnd = new Random();
            int filename = rnd.Next(1, 4);
            stu.ProfileImage = "/Profiles/default/" + filename + ".png";
            stu.Username = username;


            Notifications notify = new Notifications();
            notify.ID = Guid.NewGuid();
            notify.isRead = false;
            notify.Message = "/Profiles/default/admin.png^Admin^You have successfully created your account. You can click ask question to post your first question.";
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

        public virtual ICollection<Session> ActiveSessions { get; set; }
        public virtual ICollection<Question> QuestionsAsked { get; set; }
    }

}