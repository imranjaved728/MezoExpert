namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class experience3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Tutors", "IsCompletedProfile", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tutors", "IsCompletedProfile", c => c.Boolean(nullable: false));
        }
    }
}
