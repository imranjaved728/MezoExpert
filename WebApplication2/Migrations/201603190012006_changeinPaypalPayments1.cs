namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeinPaypalPayments1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "PostedTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "PostedTime");
        }
    }
}
