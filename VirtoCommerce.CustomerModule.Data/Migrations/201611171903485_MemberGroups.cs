namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MemberGroups : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemberGroup",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Group = c.String(maxLength: 64),
                        MemberId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Member", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.Group)
                .Index(t => t.MemberId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MemberGroup", "MemberId", "dbo.Member");
            DropIndex("dbo.MemberGroup", new[] { "MemberId" });
            DropIndex("dbo.MemberGroup", new[] { "Group" });
            DropTable("dbo.MemberGroup");
        }
    }
}
