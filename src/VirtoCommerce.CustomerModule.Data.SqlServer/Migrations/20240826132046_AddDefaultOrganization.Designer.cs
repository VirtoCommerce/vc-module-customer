﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.CustomerModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.CustomerModule.Data.SqlServer.Migrations
{
    [DbContext(typeof(CustomerDbContext))]
    [Migration("20240826132046_AddDefaultOrganization")]
    partial class AddDefaultOrganization
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.AddressEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("CountryName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("DaytimePhoneNumber")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Description")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("EveningPhoneNumber")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("FaxNumber")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Line2")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("MemberId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("Organization")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("RegionId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("RegionName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("StateProvince")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Type")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.ToTable("Address", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.EmailEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Address")
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)");

                    b.Property<bool>("IsValidated")
                        .HasColumnType("bit");

                    b.Property<string>("MemberId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Type")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("Address")
                        .HasDatabaseName("IX_Email_Address");

                    b.HasIndex("MemberId");

                    b.ToTable("Email", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.FavoriteAddressEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AddressId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("UserId")
                        .HasDatabaseName("IX_FavoriteAddress_UserId");

                    b.HasIndex("UserId", "AddressId")
                        .IsUnique()
                        .HasDatabaseName("IX_FavoriteAddress_UserId_AddressId");

                    b.ToTable("FavoriteAddress", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberDynamicPropertyObjectValueEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool?>("BooleanValue")
                        .HasColumnType("bit");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateTimeValue")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("DecimalValue")
                        .HasColumnType("decimal(18,5)");

                    b.Property<string>("DictionaryItemId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int?>("IntegerValue")
                        .HasColumnType("int");

                    b.Property<string>("Locale")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("LongTextValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ObjectId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ObjectType")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PropertyId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PropertyName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("ShortTextValue")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("ValueType")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("ObjectType", "ObjectId")
                        .HasDatabaseName("IX_MemberDynamicPropertyObjectValue_ObjectType_ObjectId");

                    b.ToTable("MemberDynamicPropertyObjectValue", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("IconUrl")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("MemberType")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Status")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("MemberType")
                        .HasDatabaseName("IX_MemberType");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_Member_Name");

                    b.ToTable("Member", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("MemberEntity");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberGroupEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Group")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("MemberId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("Group")
                        .HasDatabaseName("IX_MemberGroup_Group");

                    b.HasIndex("MemberId");

                    b.ToTable("MemberGroup", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberRelationEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AncestorId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("AncestorSequence")
                        .HasColumnType("int");

                    b.Property<string>("DescendantId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("RelationType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasDefaultValueSql("'Membership'");

                    b.HasKey("Id");

                    b.HasIndex("AncestorId");

                    b.HasIndex("DescendantId");

                    b.ToTable("MemberRelation", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.NoteEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("AuthorName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsSticky")
                        .HasColumnType("bit");

                    b.Property<string>("MemberId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifierName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Title")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.ToTable("Note", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.PhoneEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("MemberId")
                        .IsRequired()
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Number")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Type")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.ToTable("Phone", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageAltDescription")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Keyword")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Language")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("MemberId")
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("MetaDescription")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("MetaKeywords")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Title")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("MemberId");

                    b.ToTable("MemberSeoInfo", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.ContactEntity", b =>
                {
                    b.HasBaseType("VirtoCommerce.CustomerModule.Data.Model.MemberEntity");

                    b.Property<string>("About")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("About");

                    b.Property<DateTime?>("BirthDate")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("datetime2")
                        .HasColumnName("BirthDate");

                    b.Property<string>("CurrencyCode")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("DefaultBillingAddressId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("DefaultLanguage")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("DefaultLanguage");

                    b.Property<string>("DefaultOrganizationId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("DefaultOrganizationId");

                    b.Property<string>("DefaultShippingAddressId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("FirstName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("FirstName");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("FullName");

                    b.Property<bool>("IsAnonymized")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("LastName");

                    b.Property<string>("MiddleName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("MiddleName");

                    b.Property<string>("PhotoUrl")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)")
                        .HasColumnName("PhotoUrl");

                    b.Property<string>("PreferredCommunication")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("PreferredDelivery")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Salutation")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("TaxpayerId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("TimeZone")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("TimeZone");

                    b.HasDiscriminator().HasValue("ContactEntity");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.EmployeeEntity", b =>
                {
                    b.HasBaseType("VirtoCommerce.CustomerModule.Data.Model.MemberEntity");

                    b.Property<DateTime?>("BirthDate")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("datetime2")
                        .HasColumnName("BirthDate");

                    b.Property<string>("DefaultLanguage")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("DefaultLanguage");

                    b.Property<string>("DefaultOrganizationId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("DefaultOrganizationId");

                    b.Property<string>("FirstName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("FirstName");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("FullName");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("LastName");

                    b.Property<string>("MiddleName")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("MiddleName");

                    b.Property<string>("PhotoUrl")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(2083)
                        .HasColumnType("nvarchar(2083)")
                        .HasColumnName("PhotoUrl");

                    b.Property<string>("TimeZone")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasColumnName("TimeZone");

                    b.Property<string>("Type")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasDiscriminator().HasValue("EmployeeEntity");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.OrganizationEntity", b =>
                {
                    b.HasBaseType("VirtoCommerce.CustomerModule.Data.Model.MemberEntity");

                    b.Property<string>("BusinessCategory")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Description")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<string>("OwnerId")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasDiscriminator().HasValue("OrganizationEntity");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.VendorEntity", b =>
                {
                    b.HasBaseType("VirtoCommerce.CustomerModule.Data.Model.MemberEntity");

                    b.Property<string>("Description")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Description");

                    b.Property<string>("GroupName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("LogoUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("SiteUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.HasDiscriminator().HasValue("VendorEntity");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.AddressEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("Addresses")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.EmailEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("Emails")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.FavoriteAddressEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.AddressEntity", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberDynamicPropertyObjectValueEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("DynamicPropertyObjectValues")
                        .HasForeignKey("ObjectId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberGroupEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("Groups")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberRelationEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Ancestor")
                        .WithMany()
                        .HasForeignKey("AncestorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Descendant")
                        .WithMany("MemberRelations")
                        .HasForeignKey("DescendantId")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();

                    b.Navigation("Ancestor");

                    b.Navigation("Descendant");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.NoteEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("Notes")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.PhoneEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("Phones")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.SeoInfoEntity", b =>
                {
                    b.HasOne("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", "Member")
                        .WithMany("SeoInfos")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Member");
                });

            modelBuilder.Entity("VirtoCommerce.CustomerModule.Data.Model.MemberEntity", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("DynamicPropertyObjectValues");

                    b.Navigation("Emails");

                    b.Navigation("Groups");

                    b.Navigation("MemberRelations");

                    b.Navigation("Notes");

                    b.Navigation("Phones");

                    b.Navigation("SeoInfos");
                });
#pragma warning restore 612, 618
        }
    }
}
