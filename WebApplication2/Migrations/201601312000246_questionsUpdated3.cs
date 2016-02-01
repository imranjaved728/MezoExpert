namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class questionsUpdated3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Categories", "Question_QuestionID", "dbo.Questions");
            DropIndex("dbo.Categories", new[] { "Question_QuestionID" });
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
            
            DropColumn("dbo.Categories", "Question_QuestionID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Categories", "Question_QuestionID", c => c.Guid());
            DropForeignKey("dbo.QuestionCategories", "Category_CategoryID", "dbo.Categories");
            DropForeignKey("dbo.QuestionCategories", "Question_QuestionID", "dbo.Questions");
            DropIndex("dbo.QuestionCategories", new[] { "Category_CategoryID" });
            DropIndex("dbo.QuestionCategories", new[] { "Question_QuestionID" });
            DropTable("dbo.QuestionCategories");
            CreateIndex("dbo.Categories", "Question_QuestionID");
            AddForeignKey("dbo.Categories", "Question_QuestionID", "dbo.Questions", "QuestionID");
        }
    }
}
