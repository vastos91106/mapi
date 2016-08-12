namespace MAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMark : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Marks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Avatar = c.String(),
                        Lat = c.Double(nullable: false),
                        Lon = c.Double(nullable: false),
                        Radius = c.Single(nullable: false),
                        Rating = c.Int(nullable: false),
                        Description = c.String(),
                        AccountID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Accounts", t => t.AccountID, cascadeDelete: true)
                .Index(t => t.AccountID);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        To = c.Int(),
                        Text = c.String(),
                        MarkID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Marks", t => t.MarkID, cascadeDelete: true)
                .Index(t => t.MarkID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "MarkID", "dbo.Marks");
            DropForeignKey("dbo.Marks", "AccountID", "dbo.Accounts");
            DropIndex("dbo.Comments", new[] { "MarkID" });
            DropIndex("dbo.Marks", new[] { "AccountID" });
            DropTable("dbo.Comments");
            DropTable("dbo.Marks");
        }
    }
}
