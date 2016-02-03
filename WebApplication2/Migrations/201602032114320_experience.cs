namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class experience : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutors", "IsCompletedProfile", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutors", "IsCompletedProfile");
        }
    }
}
