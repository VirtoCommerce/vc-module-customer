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
            // Create table if not exists (handles both fresh installs and upgrades from old manual migration)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[CustomerOrganizationMembership]') AND type = N'U')
                CREATE TABLE [CustomerOrganizationMembership] (
                    [Id]               nvarchar(128) NOT NULL,
                    [UserId]           nvarchar(128) NOT NULL,
                    [OrganizationId]   nvarchar(128) NULL,
                    [OrganizationName] nvarchar(256) NULL,
                    [IsLocked]         bit           NOT NULL,
                    [LockoutEnd]       datetime2     NULL,
                    [CreatedDate]      datetime2     NOT NULL,
                    [ModifiedDate]     datetime2     NULL,
                    [CreatedBy]        nvarchar(64)  NULL,
                    [ModifiedBy]       nvarchar(64)  NULL,
                    CONSTRAINT [PK_CustomerOrganizationMembership] PRIMARY KEY ([Id])
                )
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[CustomerOrganizationMembershipRole]') AND type = N'U')
                CREATE TABLE [CustomerOrganizationMembershipRole] (
                    [Id]           nvarchar(128) NOT NULL,
                    [MembershipId] nvarchar(128) NOT NULL,
                    [RoleId]       nvarchar(128) NULL,
                    [RoleName]     nvarchar(256) NULL,
                    CONSTRAINT [PK_CustomerOrganizationMembershipRole] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_CustomerOrganizationMembershipRole_CustomerOrganizationMembership_MembershipId]
                        FOREIGN KEY ([MembershipId]) REFERENCES [CustomerOrganizationMembership] ([Id]) ON DELETE CASCADE
                )
            ");

            // Drop existing unique index regardless of its filter definition, then recreate with proper SQL Server filter
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_CustomerOrganizationMembership_UserId_OrganizationId'
                      AND object_id = OBJECT_ID(N'[CustomerOrganizationMembership]')
                )
                    DROP INDEX [IX_CustomerOrganizationMembership_UserId_OrganizationId]
                        ON [CustomerOrganizationMembership]
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX [IX_CustomerOrganizationMembership_UserId_OrganizationId]
                    ON [CustomerOrganizationMembership] ([UserId], [OrganizationId])
                    WHERE [OrganizationId] IS NOT NULL
            ");

            // FK support index
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_CustomerOrganizationMembershipRole_MembershipId'
                      AND object_id = OBJECT_ID(N'[CustomerOrganizationMembershipRole]')
                )
                    CREATE INDEX [IX_CustomerOrganizationMembershipRole_MembershipId]
                        ON [CustomerOrganizationMembershipRole] ([MembershipId])
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
