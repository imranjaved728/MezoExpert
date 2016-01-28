namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedStudent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "DateCreated");
        }
    }
}
