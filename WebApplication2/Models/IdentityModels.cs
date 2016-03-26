using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebApplication2.Models;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using WebApplication2.DBEntities;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Primary keys
            builder.Entity<Tutor>().HasKey(q => q.TutorID);
            builder.Entity<Category>().HasKey(q => q.CategoryID);
            builder.Entity<TutorExperties>().HasKey(q =>
                new {
                    q.TutorID,
                    q.CategoryID
                });
            

            // Relationships
            builder.Entity<TutorExperties>()
                .HasRequired(t => t.tutor)
                .WithMany(t => t.tutorExperties)
                .HasForeignKey(t => t.TutorID);

            builder.Entity<TutorExperties>()
                .HasRequired(t => t.category)
                .WithMany(t => t.tutorExperties)
                .HasForeignKey(t => t.CategoryID);



    }

        public DbSet<Student> Students { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<TutorExperties> TutorsExpertise { get; set; }
     
        public DbSet<Question> Questions { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Session> sessions { get; set; }
        public DbSet<Transaction> transcations { get; set; }
        public DbSet<PaypalPayments> payments { get; set; }
        public DbSet<ContactUs> contactus { get; set; }
        public DbSet<OnlineUsers> online { get; set; }


        public DbSet<Usera> Useras { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<PaymentRequests> paymentRequests { get; set; }
        public DbSet<NotificationConnections> notifyConnections { get; set; }
        public DbSet<Notifications> notifications { get; set; }

        public class Usera
        {
            [Key, Column(Order = 0)]
            public string UserName { get; set; }
            [Key, Column(Order =1)]
            public string SessionId { get; set; }
            public ICollection<Connection> Connections { get; set; }
        }

        public class Connection
        {
            public string ConnectionID { get; set; }
            public string UserAgent { get; set; }
            public bool Connected { get; set; }
        }
        //public DbSet<Enrollment> Enrollments { get; set; }
        //public DbSet<Course> Courses { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

   
}