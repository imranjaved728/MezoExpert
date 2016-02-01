namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class questionsUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Questions", "StudentID", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Questions", "StudentID");
        }
    }
}
