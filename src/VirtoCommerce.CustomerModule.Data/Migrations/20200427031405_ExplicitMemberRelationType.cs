using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class ExplicitMemberRelationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [MemberRelation] SET RelationType = 'Membership' WHERE RelationType IS NULL");
            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "MemberRelation",
                maxLength: 64,
                nullable: false,
                defaultValueSql: "'Membership'",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "MemberRelation",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldDefaultValueSql: "'Membership'");
            migrationBuilder.Sql("UPDATE [MemberRelation] SET RelationType = NULL WHERE RelationType = 'Membership'");
        }
    }
}
