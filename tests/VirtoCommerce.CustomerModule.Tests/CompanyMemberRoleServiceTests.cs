using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class CompanyMemberRoleServiceTests
{
    private readonly Mock<ISettingsManager> _settingsManagerMock = new();

    [Fact]
    public async Task GetAllowedRoleIdsAsync_StoreOverrideConfigured_ReturnsStoreValues()
    {
        // Arrange
        var descriptorName = ModuleConstants.Settings.General.MembershipRolesWhitelist.Name;

        _settingsManagerMock
            .Setup(x => x.GetObjectSettingAsync(descriptorName, nameof(Store), "store-1"))
            .ReturnsAsync(new ObjectSettingEntry { Id = "store-setting-1", AllowedValues = new object[] { "org-maintainer" } });

        var service = new CompanyMemberRoleService(_settingsManagerMock.Object);

        // Act
        var result = await service.GetAllowedRoleIdsAsync("store-1");

        // Assert
        Assert.Equal(["org-maintainer"], result);
    }

    [Fact]
    public async Task GetAllowedRoleIdsAsync_NoStoreOverride_FallsBackToDescriptorDefaults()
    {
        // Arrange — no DB row for this store (Id is null on the returned entry), so it must fall
        // straight to the code-defined default. There is no separate "global override" tier — matches
        // how the rest of the platform (e.g. GetStoreQueryHandler) resolves store-level settings.
        var descriptorName = ModuleConstants.Settings.General.MembershipRolesWhitelist.Name;
        var descriptorDefaults = ModuleConstants.Settings.General.MembershipRolesWhitelist.AllowedValues;

        _settingsManagerMock
            .Setup(x => x.GetObjectSettingAsync(descriptorName, nameof(Store), "store-1"))
            .ReturnsAsync(new ObjectSettingEntry { Id = null, AllowedValues = descriptorDefaults });

        var service = new CompanyMemberRoleService(_settingsManagerMock.Object);

        // Act
        var result = await service.GetAllowedRoleIdsAsync("store-1");

        // Assert
        Assert.Equal(descriptorDefaults.Length, result.Count);
        Assert.Contains("Organization maintainer", result);
        Assert.Contains("Store manager", result);
    }

    [Fact]
    public async Task GetAllowedRoleIdsAsync_NoStoreId_FallsBackToDescriptorDefaults()
    {
        // Arrange
        var service = new CompanyMemberRoleService(_settingsManagerMock.Object);

        // Act
        var result = await service.GetAllowedRoleIdsAsync(null);

        // Assert
        Assert.Equal(ModuleConstants.Settings.General.MembershipRolesWhitelist.AllowedValues.Length, result.Count);
        _settingsManagerMock.Verify(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
