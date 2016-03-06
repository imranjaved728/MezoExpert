namespace WebApplication2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sessiondelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sessions", "isTutorDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Sessions", "isStudentDelete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Sessions", "isStudentDelete");
            DropColumn("dbo.Sessions", "isTutorDeleted");
        }
    }
}
