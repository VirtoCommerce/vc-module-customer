using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationEntityTests
{
    [Fact]
    public void ToModel_WithRoles_MapsRolesCorrectly()
    {
        //Arrange
        var entity = new OrganizationEntity
        {
            Id = "org-1",
            Name = "Test Org",
            Roles =
            [
                new() { Id = "re1", OrganizationId = "org-1", RoleId = "role1", RoleName = "Admin" },
                new() { Id = "re2", OrganizationId = "org-1", RoleId = "role2", RoleName = "Editor" },
            ]
        };

        //Act
        var model = (Organization)entity.ToModel(new Organization());

        //Assert
        Assert.Equal(2, model.Roles.Count);
        Assert.Contains(model.Roles, r => r.RoleId == "role1" && r.RoleName == "Admin");
        Assert.Contains(model.Roles, r => r.RoleId == "role2" && r.RoleName == "Editor");
    }

    [Fact]
    public void ToModel_WithEmptyRoles_ReturnsEmptyList()
    {
        //Arrange
        var entity = new OrganizationEntity
        {
            Id = "org-1",
            Roles = new NullCollection<OrganizationRoleEntity>()
        };

        //Act
        var model = (Organization)entity.ToModel(new Organization());

        //Assert
        Assert.Empty(model.Roles);
    }

    [Fact]
    public void FromModel_ExistingOrg_SetsOrganizationIdOnRoles()
    {
        //Arrange
        var model = new Organization
        {
            Id = "org-existing",
            Roles =
            [
                new OrganizationRole { RoleId = "role1", RoleName = "Admin" },
                new OrganizationRole { RoleId = "role2", RoleName = "Editor" },
            ]
        };

        //Act
        var entity = (OrganizationEntity)new OrganizationEntity().FromModel(model, new PrimaryKeyResolvingMap());

        //Assert
        Assert.Equal("org-existing", entity.Id);
        Assert.All(entity.Roles, r => Assert.Equal("org-existing", r.OrganizationId));
    }

    [Fact]
    public void FromModel_WithRoles_RoleOrganizationIdMatchesEntityId()
    {
        // Verifies the convention: entity.OrganizationId = Id (entity's own Id),
        // consistent with OrganizationMembershipEntity using roleEntity.MembershipId = Id.
        //Arrange
        var model = new Organization
        {
            Id = "org-new",
            Roles = [new OrganizationRole { RoleId = "role1", RoleName = "Admin" }]
        };

        //Act
        var entity = (OrganizationEntity)new OrganizationEntity().FromModel(model, new PrimaryKeyResolvingMap());

        //Assert
        Assert.Equal("org-new", entity.Id);
        Assert.All(entity.Roles, r => Assert.Equal(entity.Id, r.OrganizationId));
    }

    [Fact]
    public void FromModel_NullRoles_LeavesRolesAsNullCollection()
    {
        //Arrange
        var model = new Organization { Id = "org-1", Roles = null };

        //Act
        var entity = (OrganizationEntity)new OrganizationEntity().FromModel(model, new PrimaryKeyResolvingMap());

        //Assert
        Assert.True(entity.Roles.IsNullCollection());
    }

    [Fact]
    public void Patch_WhenSourceHasNullCollection_DoesNotModifyTargetRoles()
    {
        // NullCollection signals "not loaded" — patch must not clear existing target roles.
        //Arrange
        var source = new OrganizationEntity
        {
            Name = "Updated Org",
            Roles = new NullCollection<OrganizationRoleEntity>()
        };

        var existingRole = new OrganizationRoleEntity { Id = "re1", RoleId = "role1", OrganizationId = "org-1" };

        var target = new OrganizationEntity
        {
            Roles = [existingRole]
        };

        //Act
        source.Patch(target);

        //Assert
        Assert.Single(target.Roles);
        Assert.Equal("role1", target.Roles[0].RoleId);
    }

    [Fact]
    public void Patch_WhenSourceHasRoles_UpdatesTargetRoles()
    {
        //Arrange
        var newRole = new OrganizationRoleEntity { Id = "re2", RoleId = "role2", RoleName = "Editor" };

        var source = new OrganizationEntity
        {
            Name = "Updated Org",
            Roles = [newRole]
        };

        var existingRole = new OrganizationRoleEntity { Id = "re1", RoleId = "role1", RoleName = "Admin" };

        var target = new OrganizationEntity
        {
            Roles = [existingRole]
        };

        //Act
        source.Patch(target);

        //Assert — re1 removed, re2 added (patch compares by Id)
        Assert.Single(target.Roles);
        Assert.Equal("role2", target.Roles[0].RoleId);
    }
}
