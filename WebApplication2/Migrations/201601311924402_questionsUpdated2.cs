namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class questionsUpdated2 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Questions", "StudentID");
            AddForeignKey("dbo.Questions", "StudentID", "dbo.Students", "StudentID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Questions", "StudentID", "dbo.Students");
            DropIndex("dbo.Questions", new[] { "StudentID" });
        }
    }
}
