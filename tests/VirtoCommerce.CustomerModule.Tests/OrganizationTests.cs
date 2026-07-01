using VirtoCommerce.CustomerModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationTests
{
    [Fact]
    public void MemberResponseGroup_Full_IncludesWithRoles()
    {
        Assert.True(MemberResponseGroup.Full.HasFlag(MemberResponseGroup.WithRoles));
    }

    [Fact]
    public void ReduceDetails_WithRolesInResponseGroup_RetainsRoles()
    {
        //Arrange
        var org = new Organization { Roles = [new OrganizationRole { RoleId = "r1" }] };

        //Act
        org.ReduceDetails(MemberResponseGroup.WithRoles.ToString());

        //Assert
        Assert.NotNull(org.Roles);
    }

    [Fact]
    public void ReduceDetails_FullResponseGroup_RetainsRoles()
    {
        //Arrange
        var org = new Organization { Roles = [new OrganizationRole { RoleId = "r1" }] };

        //Act
        org.ReduceDetails(MemberResponseGroup.Full.ToString());

        //Assert
        Assert.NotNull(org.Roles);
    }

    [Fact]
    public void ReduceDetails_NullResponseGroup_DefaultsToFull_RetainsRoles()
    {
        //Arrange
        var org = new Organization { Roles = [new OrganizationRole { RoleId = "r1" }] };

        //Act
        org.ReduceDetails(null);

        //Assert
        Assert.NotNull(org.Roles);
    }

    [Fact]
    public void ReduceDetails_WithoutRolesInResponseGroup_ClearsRoles()
    {
        //Arrange
        var org = new Organization { Roles = [new OrganizationRole { RoleId = "r1" }] };

        //Act
        org.ReduceDetails(MemberResponseGroup.WithEmails.ToString());

        //Assert
        Assert.Null(org.Roles);
    }

    [Fact]
    public void ReduceDetails_DefaultResponseGroup_ClearsRoles()
    {
        //Arrange
        var org = new Organization { Roles = [new OrganizationRole { RoleId = "r1" }] };

        //Act
        org.ReduceDetails(MemberResponseGroup.Default.ToString());

        //Assert
        Assert.Null(org.Roles);
    }

    [Fact]
    public void ReduceDetails_WhenRolesAlreadyNull_DoesNotThrow()
    {
        //Arrange
        var org = new Organization { Roles = null };

        //Act & Assert — should not throw regardless of flag
        org.ReduceDetails(MemberResponseGroup.WithEmails.ToString());

        Assert.Null(org.Roles);
    }

    [Fact]
    public void Organization_MemberType_IsOrganization()
    {
        var org = new Organization();

        Assert.Equal(nameof(Organization), org.MemberType);
    }
}
