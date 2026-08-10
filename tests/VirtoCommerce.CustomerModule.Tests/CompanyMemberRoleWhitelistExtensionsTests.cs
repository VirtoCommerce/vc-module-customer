using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class CompanyMemberRoleWhitelistExtensionsTests
{
    private static readonly string[] _orgMaintainerId = ["org-maintainer"];
    private static readonly string[] _organizationMaintainerName = ["Organization maintainer"];
    private static readonly string[] _organizationMaintainerNameUpperCase = ["ORGANIZATION MAINTAINER"];
    private static readonly string[] _purchasingAgentId = ["purchasing-agent"];

    [Fact]
    public void IsRoleAllowed_MatchesById()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.True(_orgMaintainerId.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_MatchesByName()
    {
        var role = new Role { Id = "5f3d9c1e-1234-4a5b-9abc-1234567890ab", Name = "Organization maintainer" };

        Assert.True(_organizationMaintainerName.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_MatchIsCaseInsensitive()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.True(_organizationMaintainerNameUpperCase.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_NoIdOrNameMatch_ReturnsFalse()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.False(_purchasingAgentId.IsRoleAllowed(role));
    }
}
