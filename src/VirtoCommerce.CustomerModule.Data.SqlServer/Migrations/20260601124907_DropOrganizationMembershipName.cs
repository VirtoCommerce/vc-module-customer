using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class DropOrganizationMembershipName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "CustomerOrganizationMembership");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "CustomerOrganizationMembership",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
