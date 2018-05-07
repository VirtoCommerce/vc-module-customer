namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAddressNameRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Address", "Name", c => c.String(maxLength: 2048));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Address", "Name", c => c.String(nullable: false, maxLength: 2048));
        }
    }
}
