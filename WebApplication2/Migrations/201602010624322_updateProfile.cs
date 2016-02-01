namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateProfile : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Students", "AboutMe");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Students", "AboutMe", c => c.String());
        }
    }
}
