namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedpRIMARYkey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Connections", "Usera_UserName", "dbo.Useras");
            DropIndex("dbo.Connections", new[] { "Usera_UserName" });
            DropPrimaryKey("dbo.Useras");
            AddColumn("dbo.Connections", "Usera_SessionId", c => c.String(maxLength: 128));
            AlterColumn("dbo.Useras", "SessionId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.Useras", new[] { "UserName", "SessionId" });
            CreateIndex("dbo.Connections", new[] { "Usera_UserName", "Usera_SessionId" });
            AddForeignKey("dbo.Connections", new[] { "Usera_UserName", "Usera_SessionId" }, "dbo.Useras", new[] { "UserName", "SessionId" });
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Connections", new[] { "Usera_UserName", "Usera_SessionId" }, "dbo.Useras");
            DropIndex("dbo.Connections", new[] { "Usera_UserName", "Usera_SessionId" });
            DropPrimaryKey("dbo.Useras");
            AlterColumn("dbo.Useras", "SessionId", c => c.String());
            DropColumn("dbo.Connections", "Usera_SessionId");
            AddPrimaryKey("dbo.Useras", "UserName");
            CreateIndex("dbo.Connections", "Usera_UserName");
            AddForeignKey("dbo.Connections", "Usera_UserName", "dbo.Useras", "UserName");
        }
    }
}
