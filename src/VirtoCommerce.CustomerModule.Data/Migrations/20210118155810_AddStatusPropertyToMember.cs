using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class AddStatusPropertyToMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Member",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Member");
        }
    }
}
