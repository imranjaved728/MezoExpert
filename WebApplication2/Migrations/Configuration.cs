namespace WebApplication2.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication2.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebApplication2.Models.ApplicationDbContext context)
        {

            var userRole1 = new IdentityRole { Name = "Admin", Id = Guid.NewGuid().ToString() };
            var userRole2 = new IdentityRole { Name = "Student", Id = Guid.NewGuid().ToString() };
            var userRole3 = new IdentityRole { Name = "Tutor", Id = Guid.NewGuid().ToString() };

            context.Roles.Add(userRole1);
            context.Roles.Add(userRole2);
            context.Roles.Add(userRole3);

            context.Categories.AddOrUpdate(
                p => p.CategoryID,
                new Category { CategoryID = Guid.NewGuid(), CategoryName = "Computer Science" },
                new Category { CategoryID = Guid.NewGuid(), CategoryName = "Linear Algerbra" },
                new Category { CategoryID = Guid.NewGuid(), CategoryName = "English" },
                new Category { CategoryID = Guid.NewGuid(), CategoryName = "Maths" },
                new Category { CategoryID = Guid.NewGuid(), CategoryName = "English" }
                );


            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            ApplicationUser obj1 = new ApplicationUser
            {
                UserName = "student1@abc.com",
                Email = "student1@abc.com"

            };

            ApplicationUser obj2 = new ApplicationUser
            {
                UserName = "student2@abc.com",
                Email = "student2@abc.com"
            };

            ApplicationUser obj3 = new ApplicationUser
            {
                UserName = "student3@abc.com",
                Email = "student3@abc.com"
            };

            ApplicationUser obj4 = new ApplicationUser
            {
                UserName = "tutor1@123.com",
                Email = "tutor1@123.com"
            };
            ApplicationUser obj5 = new ApplicationUser
            {
                UserName = "tutor2@123.com",
                Email = "tutor2@123.com"
            };
            ApplicationUser obj6 = new ApplicationUser
            {
                UserName = "tutor3@123.com",
                Email = "tutor3@123.com"
            };
            ApplicationUser obj7 = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com"
            };



            userManager.Create(obj1, "password");
            userManager.Create(obj2, "password");
            userManager.Create(obj3, "password");
            userManager.Create(obj4, "password");
            userManager.Create(obj5, "password");
            userManager.Create(obj6, "password");
            userManager.Create(obj7, "password");
            //userManager.AddToRole(obj1.Id, "Student");
            //userManager.AddToRole(obj2.Id, "Student");
            //userManager.AddToRole(obj3.Id, "Student");
            //userManager.AddToRole(obj4.Id, "Tutor");
            //userManager.AddToRole(obj5.Id, "Tutor");
            //userManager.AddToRole(obj6.Id, "Tutor");



            context.Tutors.AddOrUpdate(
                 p => p.TutorID,
                 new Tutor { TutorID = new Guid(obj4.Id), DateCreated = DateTime.Now, DateOfBirth = DateTime.Now },
                 new Tutor { TutorID = new Guid(obj5.Id), DateCreated = DateTime.Now, DateOfBirth = DateTime.Now },
                 new Tutor { TutorID = new Guid(obj6.Id), DateCreated = DateTime.Now, DateOfBirth = DateTime.Now }

                 );

            context.Students.AddOrUpdate(
             p => p.StudentID,
             new Student { StudentID = new Guid(obj1.Id), DateCreated = DateTime.Now },
             new Student { StudentID = new Guid(obj2.Id), DateCreated = DateTime.Now },
             new Student { StudentID = new Guid(obj3.Id), DateCreated = DateTime.Now }

             );






        }
    }
}
