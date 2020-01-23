using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.CustomerModule.Data.Migrations
{
    public partial class MemberRelationDeleteBehaviorCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberRelation_Member_DescendantId",
                table: "MemberRelation");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberRelation_Member_DescendantId",
                table: "MemberRelation",
                column: "DescendantId",
                principalTable: "Member",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberRelation_Member_DescendantId",
                table: "MemberRelation");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberRelation_Member_DescendantId",
                table: "MemberRelation",
                column: "DescendantId",
                principalTable: "Member",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
