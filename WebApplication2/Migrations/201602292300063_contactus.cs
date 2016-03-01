namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class contactus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContactUs",
                c => new
                    {
                        ContactUsID = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Username = c.String(),
                        Message = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ContactUsID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ContactUs");
        }
    }
}
