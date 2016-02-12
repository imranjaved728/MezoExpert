namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Guid(nullable: false),
                        CategoryName = c.String(),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        QuestionID = c.Guid(nullable: false),
                        StudentID = c.Guid(nullable: false),
                        TutorID = c.Guid(),
                        Title = c.String(),
                        Details = c.String(),
                        Status = c.String(),
                        Amount = c.Single(nullable: false),
                        DueDate = c.DateTime(),
                        PostedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionID)
                .ForeignKey("dbo.Students", t => t.StudentID, cascadeDelete: true)
                .Index(t => t.StudentID);
            
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        FileID = c.Guid(nullable: false),
                        QuestionID = c.Guid(),
                        ReplyID = c.Guid(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.FileID)
                .ForeignKey("dbo.Questions", t => t.QuestionID)
                .ForeignKey("dbo.Replies", t => t.ReplyID)
                .Index(t => t.QuestionID)
                .Index(t => t.ReplyID);
            
            CreateTable(
                "dbo.Sessions",
                c => new
                    {
                        SessionID = c.Guid(nullable: false),
                        TutorID = c.Guid(),
                        QuestionID = c.Guid(),
                        PostedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SessionID)
                .ForeignKey("dbo.Questions", t => t.QuestionID)
                .ForeignKey("dbo.Tutors", t => t.TutorID)
                .Index(t => t.TutorID)
                .Index(t => t.QuestionID);
            
            CreateTable(
                "dbo.Replies",
                c => new
                    {
                        ReplyID = c.Guid(nullable: false),
                        SessionID = c.Guid(nullable: false),
                        ReplierID = c.Guid(nullable: false),
                        Details = c.String(),
                        PostedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReplyID)
                .ForeignKey("dbo.Sessions", t => t.SessionID, cascadeDelete: true)
                .Index(t => t.SessionID);
            
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
                        Experience = c.String(),
                        City = c.String(),
                        Country = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        CurrentEarning = c.Single(nullable: false),
                        ProfileImage = c.String(),
                        Username = c.String(),
                        IsCompletedProfile = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TutorID);
            
            CreateTable(
                "dbo.TutorExperties",
                c => new
                    {
                        TutorID = c.Guid(nullable: false),
                        CategoryID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.TutorID, t.CategoryID })
                .ForeignKey("dbo.Categories", t => t.CategoryID, cascadeDelete: true)
                .ForeignKey("dbo.Tutors", t => t.TutorID, cascadeDelete: true)
                .Index(t => t.TutorID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        StudentID = c.Guid(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        DateOfBirth = c.DateTime(),
                        Degree = c.String(),
                        University = c.String(),
                        City = c.String(),
                        Country = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        CurrentBalance = c.Single(nullable: false),
                        ProfileImage = c.String(),
                        Username = c.String(),
                    })
                .PrimaryKey(t => t.StudentID);
            
            CreateTable(
                "dbo.Connections",
                c => new
                    {
                        ConnectionID = c.String(nullable: false, maxLength: 128),
                        UserAgent = c.String(),
                        Connected = c.Boolean(nullable: false),
                        Usera_UserName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ConnectionID)
                .ForeignKey("dbo.Useras", t => t.Usera_UserName)
                .Index(t => t.Usera_UserName);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Useras",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 128),
                        SessionId = c.String(),
                    })
                .PrimaryKey(t => t.UserName);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.QuestionCategories",
                c => new
                    {
                        Question_QuestionID = c.Guid(nullable: false),
                        Category_CategoryID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Question_QuestionID, t.Category_CategoryID })
                .ForeignKey("dbo.Questions", t => t.Question_QuestionID, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.Category_CategoryID, cascadeDelete: true)
                .Index(t => t.Question_QuestionID)
                .Index(t => t.Category_CategoryID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Connections", "Usera_UserName", "dbo.Useras");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Questions", "StudentID", "dbo.Students");
            DropForeignKey("dbo.Sessions", "TutorID", "dbo.Tutors");
            DropForeignKey("dbo.TutorExperties", "TutorID", "dbo.Tutors");
            DropForeignKey("dbo.TutorExperties", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Replies", "SessionID", "dbo.Sessions");
            DropForeignKey("dbo.Files", "ReplyID", "dbo.Replies");
            DropForeignKey("dbo.Sessions", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Files", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.QuestionCategories", "Category_CategoryID", "dbo.Categories");
            DropForeignKey("dbo.QuestionCategories", "Question_QuestionID", "dbo.Questions");
            DropIndex("dbo.QuestionCategories", new[] { "Category_CategoryID" });
            DropIndex("dbo.QuestionCategories", new[] { "Question_QuestionID" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Connections", new[] { "Usera_UserName" });
            DropIndex("dbo.TutorExperties", new[] { "CategoryID" });
            DropIndex("dbo.TutorExperties", new[] { "TutorID" });
            DropIndex("dbo.Replies", new[] { "SessionID" });
            DropIndex("dbo.Sessions", new[] { "QuestionID" });
            DropIndex("dbo.Sessions", new[] { "TutorID" });
            DropIndex("dbo.Files", new[] { "ReplyID" });
            DropIndex("dbo.Files", new[] { "QuestionID" });
            DropIndex("dbo.Questions", new[] { "StudentID" });
            DropTable("dbo.QuestionCategories");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Useras");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Connections");
            DropTable("dbo.Students");
            DropTable("dbo.TutorExperties");
            DropTable("dbo.Tutors");
            DropTable("dbo.Replies");
            DropTable("dbo.Sessions");
            DropTable("dbo.Files");
            DropTable("dbo.Questions");
            DropTable("dbo.Categories");
        }
    }
}
