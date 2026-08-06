using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class CompanyMemberRoleWhitelistExtensionsTests
{
    [Fact]
    public void IsRoleAllowed_MatchesById()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.True(new[] { "org-maintainer" }.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_MatchesByName()
    {
        var role = new Role { Id = "5f3d9c1e-1234-4a5b-9abc-1234567890ab", Name = "Organization maintainer" };

        Assert.True(new[] { "Organization maintainer" }.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_MatchIsCaseInsensitive()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.True(new[] { "ORGANIZATION MAINTAINER" }.IsRoleAllowed(role));
    }

    [Fact]
    public void IsRoleAllowed_NoIdOrNameMatch_ReturnsFalse()
    {
        var role = new Role { Id = "org-maintainer", Name = "Organization maintainer" };

        Assert.False(new[] { "purchasing-agent" }.IsRoleAllowed(role));
    }
}
