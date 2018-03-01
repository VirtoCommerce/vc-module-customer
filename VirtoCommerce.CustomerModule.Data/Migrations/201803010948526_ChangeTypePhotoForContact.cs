namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeTypePhotoForContact : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Contact", "PhotoUrl", c => c.String(maxLength: 2083));
            DropColumn("dbo.Contact", "Photo");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Contact", "Photo", c => c.Binary());
            DropColumn("dbo.Contact", "PhotoUrl");
        }
    }
}
