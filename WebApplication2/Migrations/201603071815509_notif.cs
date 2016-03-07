namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notif : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "postedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "postedTime");
        }
    }
}
