namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timezone : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutors", "Timezone", c => c.String());
            AddColumn("dbo.Students", "Timezone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "Timezone");
            DropColumn("dbo.Tutors", "Timezone");
        }
    }
}
