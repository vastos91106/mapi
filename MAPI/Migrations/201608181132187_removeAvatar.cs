namespace MAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeAvatar : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Marks", "Avatar");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Marks", "Avatar", c => c.String());
        }
    }
}
