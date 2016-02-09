namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class singalR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Useras", "SessionId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Useras", "SessionId");
        }
    }
}
