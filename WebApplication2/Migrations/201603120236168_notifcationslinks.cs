namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notifcationslinks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "link", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "link");
        }
    }
}
