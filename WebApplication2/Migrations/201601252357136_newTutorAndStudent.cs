namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newTutorAndStudent : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Enrollments", "CourseID", "dbo.Courses");
            DropForeignKey("dbo.Enrollments", "StudentID", "dbo.Students");
            DropIndex("dbo.Enrollments", new[] { "CourseID" });
            DropIndex("dbo.Enrollments", new[] { "StudentID" });
            DropPrimaryKey("dbo.Students");
            CreateTable(
                "dbo.Tutors",
                c => new
                    {
                        TutorID = c.Guid(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        DateOfBirth = c.DateTime(nullable: false),
                        Degree = c.String(),
                        University = c.String(),
                        AboutMe = c.String(),
                        City = c.String(),
                        Country = c.String(),
                        CurrentEarning = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.TutorID);
            
            AddColumn("dbo.Students", "StudentID", c => c.Guid(nullable: false));
            AddColumn("dbo.Students", "FirstName", c => c.String());
            AddColumn("dbo.Students", "DateOfBirth", c => c.DateTime(nullable: false));
            AddColumn("dbo.Students", "Degree", c => c.String());
            AddColumn("dbo.Students", "University", c => c.String());
            AddColumn("dbo.Students", "AboutMe", c => c.String());
            AddColumn("dbo.Students", "City", c => c.String());
            AddColumn("dbo.Students", "Country", c => c.String());
            AddColumn("dbo.Students", "CurrentBalance", c => c.Single(nullable: false));
            AddPrimaryKey("dbo.Students", "StudentID");
            DropColumn("dbo.Students", "ID");
            DropColumn("dbo.Students", "FirstMidName");
            DropColumn("dbo.Students", "EnrollmentDate");
            DropColumn("dbo.AspNetUsers", "HomeTown");
            DropColumn("dbo.AspNetUsers", "BirthDate");
            DropTable("dbo.Courses");
            DropTable("dbo.Enrollments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Enrollments",
                c => new
                    {
                        EnrollmentID = c.Int(nullable: false, identity: true),
                        CourseID = c.Int(nullable: false),
                        StudentID = c.Int(nullable: false),
                        Grade = c.Int(),
                    })
                .PrimaryKey(t => t.EnrollmentID);
            
            CreateTable(
                "dbo.Courses",
                c => new
                    {
                        CourseID = c.Int(nullable: false),
                        Title = c.String(nullable: false),
                        Credits = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CourseID);
            
            AddColumn("dbo.AspNetUsers", "BirthDate", c => c.DateTime());
            AddColumn("dbo.AspNetUsers", "HomeTown", c => c.String());
            AddColumn("dbo.Students", "EnrollmentDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Students", "FirstMidName", c => c.String());
            AddColumn("dbo.Students", "ID", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.Students");
            DropColumn("dbo.Students", "CurrentBalance");
            DropColumn("dbo.Students", "Country");
            DropColumn("dbo.Students", "City");
            DropColumn("dbo.Students", "AboutMe");
            DropColumn("dbo.Students", "University");
            DropColumn("dbo.Students", "Degree");
            DropColumn("dbo.Students", "DateOfBirth");
            DropColumn("dbo.Students", "FirstName");
            DropColumn("dbo.Students", "StudentID");
            DropTable("dbo.Tutors");
            AddPrimaryKey("dbo.Students", "ID");
            CreateIndex("dbo.Enrollments", "StudentID");
            CreateIndex("dbo.Enrollments", "CourseID");
            AddForeignKey("dbo.Enrollments", "StudentID", "dbo.Students", "ID", cascadeDelete: true);
            AddForeignKey("dbo.Enrollments", "CourseID", "dbo.Courses", "CourseID", cascadeDelete: true);
        }
    }
}
