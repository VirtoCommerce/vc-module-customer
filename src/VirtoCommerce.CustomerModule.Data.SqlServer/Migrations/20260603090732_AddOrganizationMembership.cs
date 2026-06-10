using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerOrganizationMembership",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrganizationMembership", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrganizationMembershipRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MembershipId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrganizationMembershipRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrganizationMembershipRole_CustomerOrganizationMembership_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "CustomerOrganizationMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganizationMembership_UserId_OrganizationId",
                table: "CustomerOrganizationMembership",
                columns: new[] { "UserId", "OrganizationId" },
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganizationMembershipRole_MembershipId",
                table: "CustomerOrganizationMembershipRole",
                column: "MembershipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerOrganizationMembershipRole");

            migrationBuilder.DropTable(
                name: "CustomerOrganizationMembership");
        }
    }
}
