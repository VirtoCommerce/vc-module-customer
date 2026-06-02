using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create tables if not exists (handles both fresh installs and upgrades from old manual migration)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `CustomerOrganizationMembership` (
                    `Id`               varchar(128) NOT NULL,
                    `UserId`           varchar(128) NOT NULL,
                    `OrganizationId`   varchar(128) NULL,
                    `OrganizationName` varchar(256) NULL,
                    `IsLocked`         tinyint(1)   NOT NULL,
                    `LockoutEnd`       datetime(6)  NULL,
                    `CreatedDate`      datetime(6)  NOT NULL,
                    `ModifiedDate`     datetime(6)  NULL,
                    `CreatedBy`        varchar(64)  NULL,
                    `ModifiedBy`       varchar(64)  NULL,
                    CONSTRAINT `PK_CustomerOrganizationMembership` PRIMARY KEY (`Id`)
                )
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `CustomerOrganizationMembershipRole` (
                    `Id`           varchar(128) NOT NULL,
                    `MembershipId` varchar(128) NOT NULL,
                    `RoleId`       varchar(128) NULL,
                    `RoleName`     varchar(256) NULL,
                    CONSTRAINT `PK_CustomerOrganizationMembershipRole` PRIMARY KEY (`Id`),
                    CONSTRAINT `FK_CustomerOrganizationMembershipRole_Membership`
                        FOREIGN KEY (`MembershipId`) REFERENCES `CustomerOrganizationMembership` (`Id`) ON DELETE CASCADE
                )
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS `IX_CustomerOrganizationMembership_UserId_OrganizationId`
                    ON `CustomerOrganizationMembership` (`UserId`, `OrganizationId`)
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS `IX_CustomerOrganizationMembershipRole_MembershipId`
                    ON `CustomerOrganizationMembershipRole` (`MembershipId`)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomerOrganizationMembershipRole");
            migrationBuilder.DropTable(name: "CustomerOrganizationMembership");
        }
    }
}
