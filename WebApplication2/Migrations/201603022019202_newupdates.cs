namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newupdates : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentRequests",
                c => new
                    {
                        PaymentId = c.Guid(nullable: false),
                        UserName = c.String(),
                        amount = c.Single(nullable: false),
                        status = c.String(),
                        postedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PaymentId);
            
            AddColumn("dbo.Tutors", "paypalEmail", c => c.String());
            AddColumn("dbo.Tutors", "moneyStatus", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutors", "moneyStatus");
            DropColumn("dbo.Tutors", "paypalEmail");
            DropTable("dbo.PaymentRequests");
        }
    }
}
