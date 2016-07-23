namespace MAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixget : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AccountTypes", "AuthType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AccountTypes", "AuthType");
        }
    }
}
