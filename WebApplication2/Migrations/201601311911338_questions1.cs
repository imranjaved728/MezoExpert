namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class questions1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Guid(nullable: false),
                        CategoryName = c.String(),
                        Question_QuestionID = c.Guid(),
                    })
                .PrimaryKey(t => t.CategoryID)
                .ForeignKey("dbo.Questions", t => t.Question_QuestionID)
                .Index(t => t.Question_QuestionID);
            
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
                "dbo.Questions",
                c => new
                    {
                        QuestionID = c.Guid(nullable: false),
                        TutorID = c.Guid(),
                        Title = c.String(),
                        Details = c.String(),
                        Status = c.String(),
                        Amount = c.Single(nullable: false),
                        DueDate = c.DateTime(),
                        PostedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.QuestionID);
            
            CreateTable(
                "dbo.Replies",
                c => new
                    {
                        ReplyID = c.Guid(nullable: false),
                        QuestionID = c.Guid(nullable: false),
                        ReplierID = c.Guid(nullable: false),
                        Details = c.String(),
                        PostedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ReplyID)
                .ForeignKey("dbo.Questions", t => t.QuestionID, cascadeDelete: true)
                .Index(t => t.QuestionID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Replies", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Files", "ReplyID", "dbo.Replies");
            DropForeignKey("dbo.Files", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Categories", "Question_QuestionID", "dbo.Questions");
            DropIndex("dbo.Replies", new[] { "QuestionID" });
            DropIndex("dbo.Files", new[] { "ReplyID" });
            DropIndex("dbo.Files", new[] { "QuestionID" });
            DropIndex("dbo.Categories", new[] { "Question_QuestionID" });
            DropTable("dbo.Replies");
            DropTable("dbo.Questions");
            DropTable("dbo.Files");
            DropTable("dbo.Categories");
        }
    }
}
