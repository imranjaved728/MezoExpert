namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sesssionNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "sessionId", c => c.String());
            AddColumn("dbo.Notifications", "counts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "counts");
            DropColumn("dbo.Notifications", "sessionId");
        }
    }
}
