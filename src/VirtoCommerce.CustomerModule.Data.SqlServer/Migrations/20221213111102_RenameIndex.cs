using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.SqlServer.Migrations
{
    public partial class RenameIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ObjectType_ObjectId",
                table: "MemberDynamicPropertyObjectValue",
                newName: "IX_MemberDynamicPropertyObjectValue_ObjectType_ObjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_MemberDynamicPropertyObjectValue_ObjectType_ObjectId",
                table: "MemberDynamicPropertyObjectValue",
                newName: "IX_ObjectType_ObjectId");
        }
    }
}
