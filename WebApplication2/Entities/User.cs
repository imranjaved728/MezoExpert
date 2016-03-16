using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Entities
{
    public abstract class User
    {
        //getters and setter
        protected Guid ID { get; set; }
        protected string FirstName { get; set; }
        protected string LastName { get; set; }
        protected DateTime? DateOfBirth { get; set; }
        protected string Degree { get; set; }
        protected string University { get; set; }
        protected string City { get; set; }
        protected string Country { get; set; }
        protected DateTime DateCreated { get; set; }
        protected string ProfileImage { get; set; }
        protected string Username { get; set; }
        protected string role { get; set; }

        public virtual ICollection<Notifications> notifications { get; set; }
        protected bool login(string username, string password) { return false; }
        protected bool Register(User obj) { return false; }

        public User()
        {
            ID = new Guid();
        }
    }
}