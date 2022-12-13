using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.PostgreSql.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    FullName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DefaultLanguage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TaxpayerId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PreferredDelivery = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PreferredCommunication = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DefaultShippingAddressId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DefaultBillingAddressId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: true),
                    Salutation = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsAnonymized = table.Column<bool>(type: "boolean", nullable: true),
                    About = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BusinessCategory = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OwnerId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    GroupName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Member", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Line1 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Line2 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StateProvince = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CountryName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RegionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RegionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DaytimePhoneNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EveningPhoneNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FaxNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Organization = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Email",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Address = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    IsValidated = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Email_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberDynamicPropertyObjectValue",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ObjectType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Locale = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ValueType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ShortTextValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LongTextValue = table.Column<string>(type: "text", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "numeric(18,5)", nullable: true),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PropertyId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DictionaryItemId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberDynamicPropertyObjectValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberDynamicPropertyObjectValue_Member_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberGroup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Group = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberGroup_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberRelation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AncestorSequence = table.Column<int>(type: "integer", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValueSql: "'Membership'"),
                    AncestorId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DescendantId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberRelation_Member_AncestorId",
                        column: x => x.AncestorId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberRelation_Member_DescendantId",
                        column: x => x.DescendantId,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MemberSeoInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Keyword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MetaDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImageAltDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSeoInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberSeoInfo_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Note",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ModifierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    IsSticky = table.Column<bool>(type: "boolean", nullable: false),
                    OuterId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Note_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phone",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MemberId = table.Column<string>(type: "character varying(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Phone_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_MemberId",
                table: "Address",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Email_Address",
                table: "Email",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Email_MemberId",
                table: "Email",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_Name",
                table: "Member",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MemberType",
                table: "Member",
                column: "MemberType");

            migrationBuilder.CreateIndex(
                name: "IX_MemberDynamicPropertyObjectValue_ObjectId",
                table: "MemberDynamicPropertyObjectValue",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberDynamicPropertyObjectValue_ObjectType_ObjectId",
                table: "MemberDynamicPropertyObjectValue",
                columns: new[] { "ObjectType", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberGroup_Group",
                table: "MemberGroup",
                column: "Group");

            migrationBuilder.CreateIndex(
                name: "IX_MemberGroup_MemberId",
                table: "MemberGroup",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRelation_AncestorId",
                table: "MemberRelation",
                column: "AncestorId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberRelation_DescendantId",
                table: "MemberRelation",
                column: "DescendantId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSeoInfo_MemberId",
                table: "MemberSeoInfo",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_MemberId",
                table: "Note",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Phone_MemberId",
                table: "Phone",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Email");

            migrationBuilder.DropTable(
                name: "MemberDynamicPropertyObjectValue");

            migrationBuilder.DropTable(
                name: "MemberGroup");

            migrationBuilder.DropTable(
                name: "MemberRelation");

            migrationBuilder.DropTable(
                name: "MemberSeoInfo");

            migrationBuilder.DropTable(
                name: "Note");

            migrationBuilder.DropTable(
                name: "Phone");

            migrationBuilder.DropTable(
                name: "Member");
        }
    }
}
