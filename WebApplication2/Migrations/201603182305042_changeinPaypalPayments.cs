namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeinPaypalPayments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PaypalPayments", "PostedTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaypalPayments", "PostedTime");
        }
    }
}
