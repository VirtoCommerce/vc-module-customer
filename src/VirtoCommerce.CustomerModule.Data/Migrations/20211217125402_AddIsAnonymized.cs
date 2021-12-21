using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class AddIsAnonymized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymized",
                table: "Member",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymized",
                table: "Member");
        }
    }
}
