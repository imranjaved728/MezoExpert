namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rating : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Sessions", "ratings", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Sessions", "ratings", c => c.String());
        }
    }
}
