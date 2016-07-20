namespace MAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Location = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AccountTypes",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        AccountID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Accounts", t => t.AccountID, cascadeDelete: true)
                .Index(t => t.AccountID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccountTypes", "AccountID", "dbo.Accounts");
            DropIndex("dbo.AccountTypes", new[] { "AccountID" });
            DropTable("dbo.AccountTypes");
            DropTable("dbo.Accounts");
        }
    }
}
