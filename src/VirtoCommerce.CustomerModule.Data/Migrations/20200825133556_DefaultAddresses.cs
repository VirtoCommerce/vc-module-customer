using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class DefaultAddresses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultBillingAddressId",
                table: "Member",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultShippingAddressId",
                table: "Member",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultBillingAddressId",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "DefaultShippingAddressId",
                table: "Member");
        }
    }
}
