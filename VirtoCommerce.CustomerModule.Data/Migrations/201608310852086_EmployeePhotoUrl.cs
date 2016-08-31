namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmployeePhotoUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employee", "PhotoUrl", c => c.String(maxLength: 2083));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employee", "PhotoUrl");
        }
    }
}
