namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Connections",
                c => new
                    {
                        ConnectionID = c.String(nullable: false, maxLength: 128),
                        UserAgent = c.String(),
                        Connected = c.Boolean(nullable: false),
                        Usera_UserName = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ConnectionID)
                .ForeignKey("dbo.Useras", t => t.Usera_UserName)
                .Index(t => t.Usera_UserName);
            
            CreateTable(
                "dbo.Useras",
                c => new
                    {
                        UserName = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.UserName);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Connections", "Usera_UserName", "dbo.Useras");
            DropIndex("dbo.Connections", new[] { "Usera_UserName" });
            DropTable("dbo.Useras");
            DropTable("dbo.Connections");
        }
    }
}
