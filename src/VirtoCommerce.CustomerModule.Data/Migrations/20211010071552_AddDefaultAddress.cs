using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class AddDefaultAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DefaultAddress",
                table: "Address",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAddress",
                table: "Address");
        }
    }
}
