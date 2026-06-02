using System;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationMembershipTests
{
    public static TheoryData<bool, DateTime?, bool> IsCurrentlyLockedData => new()
    {
        { false, null,                           false },
        { true,  null,                           true  },
        { true,  DateTime.UtcNow.AddDays(1),     true  },
        { true,  DateTime.UtcNow.AddDays(-1),    false },
    };

    [Theory]
    [MemberData(nameof(IsCurrentlyLockedData))]
    public void IsCurrentlyLocked_ReturnsExpected(bool isLocked, DateTime? lockoutEnd, bool expected)
    {
        var membership = new OrganizationMembership { IsLocked = isLocked, LockoutEnd = lockoutEnd };
        Assert.Equal(expected, membership.IsCurrentlyLocked);
    }

    [Fact]
    public void OrganizationMembershipEntity_ToModel_MapsAllFields()
    {
        //Arrange
        var roleEntity = new OrganizationMembershipRoleEntity { Id = "r1", RoleId = "role1", RoleName = "Admin" };
        var entity = new OrganizationMembershipEntity
        {
            Id = "id1",
            UserId = "user1",
            OrganizationId = "org1",
            IsLocked = true,
            LockoutEnd = new DateTime(2030, 1, 1),
            Roles = [roleEntity]
        };

        //Act
        var model = entity.ToModel(new OrganizationMembership());

        //Assert
        Assert.Equal(entity.Id, model.Id);
        Assert.Equal(entity.UserId, model.UserId);
        Assert.Equal(entity.OrganizationId, model.OrganizationId);
        Assert.Equal(entity.IsLocked, model.IsLocked);
        Assert.Equal(entity.LockoutEnd, model.LockoutEnd);
        Assert.Single(model.Roles);
        Assert.Equal(roleEntity.RoleId, model.Roles[0].RoleId);
        Assert.Equal(roleEntity.RoleName, model.Roles[0].RoleName);
    }

    [Fact]
    public void OrganizationMembershipEntity_Patch_UpdatesAllFields()
    {
        //Arrange
        var source = new OrganizationMembershipEntity
        {
            UserId = "user1",
            OrganizationId = "org1",
            IsLocked = true,
            LockoutEnd = new DateTime(2030, 6, 1)
        };
        var target = new OrganizationMembershipEntity { IsLocked = false, LockoutEnd = null };

        //Act
        source.Patch(target);

        //Assert
        Assert.Equal(source.IsLocked, target.IsLocked);
        Assert.Equal(source.LockoutEnd, target.LockoutEnd);
        Assert.Equal(source.UserId, target.UserId);
        Assert.Equal(source.OrganizationId, target.OrganizationId);
    }
}
