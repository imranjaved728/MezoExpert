namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updates : DbMigration
    {
        public override void Up()
        {
            
            
            //AddColumn("dbo.Tutors", "paypalEmail", c => c.String());
            //AddColumn("dbo.Tutors", "moneyStatus", c => c.String());
        }
        
        public override void Down()
        {
           // DropColumn("dbo.Tutors", "moneyStatus");
            //DropColumn("dbo.Tutors", "paypalEmail");
            
        }
    }
}
