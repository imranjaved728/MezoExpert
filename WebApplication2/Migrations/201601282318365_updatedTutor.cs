namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedTutor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutors", "DateCreated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutors", "DateCreated");
        }
    }
}
