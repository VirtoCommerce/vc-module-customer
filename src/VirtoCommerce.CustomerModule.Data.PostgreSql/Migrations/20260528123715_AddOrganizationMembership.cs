using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create tables if not exists (handles both fresh installs and upgrades from old manual migration)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CustomerOrganizationMembership"" (
                    ""Id""               character varying(128) NOT NULL,
                    ""UserId""           character varying(128) NOT NULL,
                    ""OrganizationId""   character varying(128) NULL,
                    ""OrganizationName"" character varying(256) NULL,
                    ""IsLocked""         boolean                NOT NULL,
                    ""LockoutEnd""       timestamp with time zone NULL,
                    ""CreatedDate""      timestamp with time zone NOT NULL,
                    ""ModifiedDate""     timestamp with time zone NULL,
                    ""CreatedBy""        character varying(64)  NULL,
                    ""ModifiedBy""       character varying(64)  NULL,
                    CONSTRAINT ""PK_CustomerOrganizationMembership"" PRIMARY KEY (""Id"")
                )
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""CustomerOrganizationMembershipRole"" (
                    ""Id""           character varying(128) NOT NULL,
                    ""MembershipId"" character varying(128) NOT NULL,
                    ""RoleId""       character varying(128) NULL,
                    ""RoleName""     character varying(256) NULL,
                    CONSTRAINT ""PK_CustomerOrganizationMembershipRole"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_CustomerOrganizationMembershipRole_Membership""
                        FOREIGN KEY (""MembershipId"") REFERENCES ""CustomerOrganizationMembership"" (""Id"") ON DELETE CASCADE
                )
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_CustomerOrganizationMembership_UserId_OrganizationId""
                    ON ""CustomerOrganizationMembership"" (""UserId"", ""OrganizationId"")
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_CustomerOrganizationMembershipRole_MembershipId""
                    ON ""CustomerOrganizationMembershipRole"" (""MembershipId"")
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
