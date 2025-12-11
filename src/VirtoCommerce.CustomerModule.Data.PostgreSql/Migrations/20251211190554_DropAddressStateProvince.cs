using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class DropAddressStateProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateProvince",
                table: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StateProvince",
                table: "Address",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
