namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class onlneusers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OnlineUsers",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Username);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OnlineUsers");
        }
    }
}
