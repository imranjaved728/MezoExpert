namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationConnections",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        userName = c.String(),
                        connectionId = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NotificationConnections");
        }
    }
}
