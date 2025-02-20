using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentOrganizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentOrganizationId",
                table: "Member",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentOrganizationId",
                table: "Member");
        }
    }
}
