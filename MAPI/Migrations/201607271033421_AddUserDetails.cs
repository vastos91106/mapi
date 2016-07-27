namespace MAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Accounts", "Name");
        }
    }
}
