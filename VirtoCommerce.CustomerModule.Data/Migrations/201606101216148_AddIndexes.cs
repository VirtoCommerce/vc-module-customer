namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Email", "Address", c => c.String(maxLength: 254));
            CreateIndex("dbo.Member", "MemberType");
            CreateIndex("dbo.Member", "Name");
            CreateIndex("dbo.Email", "Address");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Email", new[] { "Address" });
            DropIndex("dbo.Member", new[] { "Name" });
            DropIndex("dbo.Member", new[] { "MemberType" });
            AlterColumn("dbo.Email", "Address", c => c.String());
        }
    }
}
