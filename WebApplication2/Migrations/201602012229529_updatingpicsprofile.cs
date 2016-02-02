namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatingpicsprofile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "ProfileImage", c => c.String());
            AddColumn("dbo.Tutors", "ProfileImage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutors", "ProfileImage");
            DropColumn("dbo.Students", "ProfileImage");
        }
    }
}
